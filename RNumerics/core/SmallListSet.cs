using System;
using System.Collections.Generic;

namespace RNumerics
{
	/// <summary>
	/// SmallListSet stores a set of short integer-valued variable-size lists.
	/// The lists are encoded into a few large DVector buffers, with internal pooling,
	/// so adding/removing lists usually does not involve any new or delete ops.
	/// 
	/// The lists are stored in two parts. The first N elements are stored in a linear
	/// subset of a dvector. If the list spills past these N elements, the extra elements
	/// are stored in a linked list (which is also stored in a flat array).
	/// 
	/// Each list stores its count, so list-size operations are constant time.
	/// All the internal "pointers" are 32-bit.
	/// </summary>
	public sealed class SmallListSet
	{
		const int NULL = -1;

		const int BLOCKSIZE = 8;
		const int BLOCK_LIST_OFFSET = BLOCKSIZE + 1;
		readonly DVector<int> _list_heads;        // each "list" is stored as index of first element in block-store (like a pointer)

		readonly DVector<int> _block_store;       // flat buffer used to store per-list initial block
												  // blocks are BLOCKSIZE+2 long, elements are [CurrentCount, item0...itemN, LinkedListPtr]


		readonly DVector<int> _free_blocks;       // list of free blocks, indices into block_store
		int _allocated_count = 0;
		readonly DVector<int> _linked_store;      // flat buffer used for linked-list elements,
												  // each element is [value, next_ptr]

		int _free_head_ptr;              // index of first free element in linked_store


		public SmallListSet() {
			_list_heads = new DVector<int>();
			_linked_store = new DVector<int>();
			_free_head_ptr = NULL;
			_block_store = new DVector<int>();
			_free_blocks = new DVector<int>();
		}


		public SmallListSet(in SmallListSet copy) {
			_linked_store = new DVector<int>(copy._linked_store);
			_free_head_ptr = copy._free_head_ptr;
			_list_heads = new DVector<int>(copy._list_heads);
			_block_store = new DVector<int>(copy._block_store);
			_free_blocks = new DVector<int>(copy._free_blocks);
		}


		/// <summary>
		/// returns largest current list_index
		/// </summary>
		public int Size => _list_heads.Size;

		/// <summary>
		/// resize the list-of-lists
		/// </summary>
		public void Resize(in int new_size) {
			var cur_size = _list_heads.Size;
			if (new_size > cur_size) {
				_list_heads.Resize(new_size);
				for (var k = cur_size; k < new_size; ++k) {
					_list_heads[k] = NULL;
				}
			}
		}


		/// <summary>
		/// create a new list at list_index
		/// </summary>
		public void AllocateAt(in int list_index) {
			if (list_index >= _list_heads.Size) {
				var j = _list_heads.Size;
				_list_heads.Insert(NULL, list_index);
				// need to set intermediate values to null! 
				while (j < list_index) {
					_list_heads[j] = NULL;
					j++;
				}
			}
			else {
				if (_list_heads[list_index] != NULL) {
					throw new Exception("SmallListSet: list at " + list_index + " is not empty!");
				}
			}
		}


		/// <summary>
		/// insert val into list at list_index. 
		/// </summary>
		public void Insert(in int list_index, in int val) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr == NULL) {
				block_ptr = Allocate_block();
				_block_store[block_ptr] = 0;
				_list_heads[list_index] = block_ptr;
			}

			var N = _block_store[block_ptr];
			if (N < BLOCKSIZE) {
				_block_store[block_ptr + N + 1] = val;
			}
			else {
				// spill to linked list
				var cur_head = _block_store[block_ptr + BLOCK_LIST_OFFSET];

				if (_free_head_ptr == NULL) {
					// allocate new linkedlist node
					var new_ptr = _linked_store.Size;
					_linked_store.Add(val);
					_linked_store.Add(cur_head);
					_block_store[block_ptr + BLOCK_LIST_OFFSET] = new_ptr;
				}
				else {
					// pull from free list
					var free_ptr = _free_head_ptr;
					_free_head_ptr = _linked_store[free_ptr + 1];
					_linked_store[free_ptr] = val;
					_linked_store[free_ptr + 1] = cur_head;
					_block_store[block_ptr + BLOCK_LIST_OFFSET] = free_ptr;
				}
			}

			// count new element
			_block_store[block_ptr] += 1;
		}



		/// <summary>
		/// remove val from the list at list_index. return false if val was not in list.
		/// </summary>
		public bool Remove(in int list_index, in int val) {
			var block_ptr = _list_heads[list_index];
			var N = _block_store[block_ptr];


			var iEnd = block_ptr + Math.Min(N, BLOCKSIZE);
			for (var i = block_ptr + 1; i <= iEnd; ++i) {

				if (_block_store[i] == val) {
					for (var j = i + 1; j <= iEnd; ++j)     // shift left
{
						_block_store[j - 1] = _block_store[j];
					}
					//block_store[iEnd] = -2;     // OPTIONAL

					if (N > BLOCKSIZE) {
						var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
						_block_store[block_ptr + BLOCK_LIST_OFFSET] = _linked_store[cur_ptr + 1];  // point to cur->next
						_block_store[iEnd] = _linked_store[cur_ptr];
						Add_free_link(cur_ptr);
					}

					_block_store[block_ptr] -= 1;
					return true;
				}

			}

			// search list
			if (N > BLOCKSIZE) {
				if (Remove_from_linked_list(block_ptr, val)) {
					_block_store[block_ptr] -= 1;
					return true;
				}
			}

			return false;
		}



		/// <summary>
		/// move list at from_index to to_index
		/// </summary>
		public void Move(in int from_index, in int to_index) {
			if (_list_heads[to_index] != NULL) {
				throw new Exception("SmallListSet.MoveTo: list at " + to_index + " is not empty!");
			}

			if (_list_heads[from_index] == NULL) {
				throw new Exception("SmallListSet.MoveTo: list at " + from_index + " is empty!");
			}

			_list_heads[to_index] = _list_heads[from_index];
			_list_heads[from_index] = NULL;
		}






		/// <summary>
		/// remove all elements from list at list_index
		/// </summary>
		public void Clear(in int list_index) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr != NULL) {
				var N = _block_store[block_ptr];

				// if we have spilled to linked-list, free nodes
				if (N > BLOCKSIZE) {
					var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
					while (cur_ptr != NULL) {
						var free_ptr = cur_ptr;
						cur_ptr = _linked_store[cur_ptr + 1];
						Add_free_link(free_ptr);
					}
					_block_store[block_ptr + BLOCK_LIST_OFFSET] = NULL;
				}

				// free our block
				_block_store[block_ptr] = 0;
				_free_blocks.Push_back(block_ptr);
				_list_heads[list_index] = NULL;
			}

		}


		/// <summary>
		/// return size of list at list_index
		/// </summary>
		public int Count(in int list_index) {
			var block_ptr = _list_heads[list_index];
			return (block_ptr == NULL) ? 0 : _block_store[block_ptr];
		}


		/// <summary>
		/// search for val in list at list_index
		/// </summary>
		public bool Contains(in int list_index, in int val) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr != NULL) {
				var N = _block_store[block_ptr];
				if (N < BLOCKSIZE) {
					var iEnd = block_ptr + N;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						if (_block_store[i] == val) {
							return true;
						}
					}
				}
				else {
					// we spilled to linked list, have to iterate through it as well
					var iEnd = block_ptr + BLOCKSIZE;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						if (_block_store[i] == val) {
							return true;
						}
					}
					var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
					while (cur_ptr != NULL) {
						if (_linked_store[cur_ptr] == val) {
							return true;
						}

						cur_ptr = _linked_store[cur_ptr + 1];
					}
				}
			}
			return false;
		}


		/// <summary>
		/// return the first item in the list at list_index (no zero-size-list checking)
		/// </summary>
		public int First(in int list_index) {
			var block_ptr = _list_heads[list_index];
			return _block_store[block_ptr + 1];
		}


		/// <summary>
		/// iterate over the values of list at list_index
		/// </summary>
		public IEnumerable<int> ValueItr(int list_index) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr != NULL) {
				var N = _block_store[block_ptr];
				if (N < BLOCKSIZE) {
					var iEnd = block_ptr + N;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						yield return _block_store[i];
					}
				}
				else {
					// we spilled to linked list, have to iterate through it as well
					var iEnd = block_ptr + BLOCKSIZE;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						yield return _block_store[i];
					}

					var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
					while (cur_ptr != NULL) {
						yield return _linked_store[cur_ptr];
						cur_ptr = _linked_store[cur_ptr + 1];
					}
				}
			}
		}


		/// <summary>
		/// search for findF(list_value) == true, of list at list_index, and return list_value
		/// </summary>
		public int Find(in int list_index, in Func<int, bool> findF, in int invalidValue = -1) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr != NULL) {
				var N = _block_store[block_ptr];
				if (N < BLOCKSIZE) {
					var iEnd = block_ptr + N;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						var val = _block_store[i];
						if (findF(val)) {
							return val;
						}
					}
				}
				else {
					// we spilled to linked list, have to iterate through it as well
					var iEnd = block_ptr + BLOCKSIZE;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						var val = _block_store[i];
						if (findF(val)) {
							return val;
						}
					}
					var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
					while (cur_ptr != NULL) {
						var val = _linked_store[cur_ptr];
						if (findF(val)) {
							return val;
						}

						cur_ptr = _linked_store[cur_ptr + 1];
					}
				}
			}
			return invalidValue;
		}





		/// <summary>
		/// search for findF(list_value) == true, of list at list_index, and replace with new_value.
		/// returns false if not found
		/// </summary>
		public bool Replace(in int list_index, in Func<int, bool> findF, in int new_value) {
			var block_ptr = _list_heads[list_index];
			if (block_ptr != NULL) {
				var N = _block_store[block_ptr];
				if (N < BLOCKSIZE) {
					var iEnd = block_ptr + N;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						var val = _block_store[i];
						if (findF(val)) {
							_block_store[i] = new_value;
							return true;
						}
					}
				}
				else {
					// we spilled to linked list, have to iterate through it as well
					var iEnd = block_ptr + BLOCKSIZE;
					for (var i = block_ptr + 1; i <= iEnd; ++i) {
						var val = _block_store[i];
						if (findF(val)) {
							_block_store[i] = new_value;
							return true;
						}
					}
					var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
					while (cur_ptr != NULL) {
						var val = _linked_store[cur_ptr];
						if (findF(val)) {
							_linked_store[cur_ptr] = new_value;
							return true;
						}
						cur_ptr = _linked_store[cur_ptr + 1];
					}
				}
			}
			return false;
		}



		// grab a block from the free list, or allocate a new one
		private int Allocate_block() {
			var nfree = _free_blocks.Size;
			if (nfree > 0) {
				var ptr = _free_blocks[nfree - 1];
				_free_blocks.Pop_back();
				return ptr;
			}
			var nsize = _block_store.Size;
			_block_store.Insert(NULL, nsize + BLOCK_LIST_OFFSET);
			_block_store[nsize] = 0;
			_allocated_count++;
			return nsize;
		}


		// push a link-node onto the free list
		void Add_free_link(in int ptr) {
			_linked_store[ptr + 1] = _free_head_ptr;
			_free_head_ptr = ptr;
		}


		// remove val from the linked-list attached to block_ptr
		bool Remove_from_linked_list(in int block_ptr, in int val) {
			var cur_ptr = _block_store[block_ptr + BLOCK_LIST_OFFSET];
			var prev_ptr = NULL;
			while (cur_ptr != NULL) {
				if (_linked_store[cur_ptr] == val) {
					var next_ptr = _linked_store[cur_ptr + 1];
					if (prev_ptr == NULL) {
						_block_store[block_ptr + BLOCK_LIST_OFFSET] = next_ptr;
					}
					else {
						_linked_store[prev_ptr + 1] = next_ptr;
					}
					Add_free_link(cur_ptr);
					return true;
				}
				prev_ptr = cur_ptr;
				cur_ptr = _linked_store[cur_ptr + 1];
			}
			return false;
		}



		public string MemoryUsage
		{
			get {
				return string.Format("ListSize {0}  Blocks Count {1} Free {2} Mem {3}kb  Linked Mem {4}kb",
					_list_heads.Size, _allocated_count, _free_blocks.Size * sizeof(int) / 1024, _block_store.Size, _linked_store.Size * sizeof(int) / 1024);
			}
		}


	}
}
