using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;


namespace RNumerics
{
	/// <summary>
	/// HBitArray is a hierarchical variant of BitArray. Basically the idea
	/// is to make a tree of 32-bit blocks, where at level N, a '0' means that
	/// no bits are true in level N-1. This means we can more efficiently iterate
	/// over the bit set. 
	/// 
	/// Uses more memory than BitArray, but each tree level is divided by 32, so
	/// it is better than NlogN
	/// </summary>
	public class HBitArray : IEnumerable<int>
	{
		struct MyBitVector32
		{
			public bool this[int i]
			{
				get => (Data & (1 << i)) != 0;
				set {
					if (value) {
						Data |= 1 << i;
					}
					else {
						Data &= ~(1 << i);
					}
				}
			}
			public int Data { get; private set; }
		}

		readonly MyBitVector32[] _bits;

		struct Layer
		{
			public MyBitVector32[] layer_bits;
		}

		readonly Layer[] _layers;
		readonly int _layerCount;

		public HBitArray(int maxIndex) {
			Count = maxIndex;
			var base_count = maxIndex / 32;
			if (maxIndex % 32 != 0) {
				base_count++;
			}

			_bits = new MyBitVector32[base_count];
			TrueCount = 0;

			_layerCount = 2;
			_layers = new Layer[_layerCount];

			var prev_size = _bits.Length;
			for (var i = 0; i < _layerCount; ++i) {
				var cur_size = prev_size / 32;
				if (prev_size % 32 != 0) {
					cur_size++;
				}

				_layers[i].layer_bits = new MyBitVector32[cur_size];
				prev_size = cur_size;
			}
		}


		public bool this[int i]
		{
			get => Get(i);
			set => Set(i, value);
		}


		public int Count { get; }

		public int TrueCount { get; private set; }


		public bool Contains(int i) {
			return Get(i) == true;
		}

		public void Add(int i) {
			Set(i, true);
		}

		public void Set(int i, bool value) {
			var byte_i = i / 32;
			var byte_o = i - (32 * byte_i);

			Debug.Assert(byte_o < 32);

			if (value == true) {
				if (_bits[byte_i][byte_o] == false) {
					_bits[byte_i][byte_o] = true;
					TrueCount++;

					// [TODO] only need to propagate up if our current field was zero
					for (var li = 0; li < _layerCount; ++li) {
						var layer_i = byte_i / 32;
						var layer_o = byte_i - (32 * layer_i);
						_layers[li].layer_bits[layer_i][layer_o] = true;
						byte_i = layer_i;
					}
				}

			}
			else {
				if (_bits[byte_i][byte_o] == true) {
					_bits[byte_i][byte_o] = false;
					TrueCount--;

					// [RMS] [June 6 2017] not sure if this comment is still true or not. Need to experiment.
					// [TODO] only need to propagate up if our current field becomes zero
					//ACK NO THIS IS WRONG! only clear parent bit if our entire bit is zero!

					for (var li = 0; li < _layerCount; ++li) {
						var layer_i = byte_i / 32;
						var layer_o = byte_i - (32 * layer_i);
						_layers[li].layer_bits[layer_i][layer_o] = false;
						byte_i = layer_i;
					}
				}
			}
		}


		public bool Get(int i) {
			var byte_i = i / 32;
			var byte_o = i - (32 * byte_i);
			return _bits[byte_i][byte_o];
		}



		public IEnumerator<int> GetEnumerator() {
			if (TrueCount > Count / 3) {
				for (var bi = 0; bi < _bits.Length; ++bi) {
					var d = _bits[bi].Data;
					var dmask = 1;
					var maxj = (bi == _bits.Length - 1) ? Count % 32 : 32;
					for (var j = 0; j < maxj; ++j) {
						if ((d & dmask) != 0) {
							yield return (bi * 32) + j;
						}

						dmask <<= 1;
					}
				}

			}
			else {
				for (var ai = 0; ai < _layers[1].layer_bits.Length; ++ai) {
					if (_layers[1].layer_bits[ai].Data == 0) {
						continue;
					}

					for (var aj = 0; aj < 32; aj++) {
						if (_layers[1].layer_bits[ai][aj]) {

							var bi = (ai * 32) + aj;
							Debug.Assert(_layers[0].layer_bits[bi].Data != 0);
							for (var bj = 0; bj < 32; bj++) {
								if (_layers[0].layer_bits[bi][bj]) {
									var i = (bi * 32) + bj;

									var d = _bits[i].Data;
									var dmask = 1;
									for (var j = 0; j < 32; ++j) {
										if ((d & dmask) != 0) {
											yield return (i * 32) + j;
										}

										dmask <<= 1;
									}
								}
							}
						}
					}
				}
			}
		}



		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
