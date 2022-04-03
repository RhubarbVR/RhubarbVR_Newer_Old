using System;
using System.Collections;


namespace RNumerics
{
	/// <summary>
	/// 2D MarchingQuads polyline extraction from scalar field
	/// [TODO] this is very, very old code. Should at minimum rewrite using current
	/// vector classes/etc.
	/// </summary>
	public class MarchingQuads
	{
		AxisAlignedBox2f _bounds;
		float _fXShift;
		float _fYShift;
		float _fScale;

		int _nCells;
		float _fCellSize;

		static readonly float _fValueSentinel = 9999999.0f;
		readonly float _fIsoValue;

		const int LEFT = 0x1;
		const int TOP = 0x2;
		const int RIGHT = 0x4;
		const int BOTTOM = 0x8;
		const int ALL = 0xF;

		struct Cell
		{
			uint _nPosition;    // 16 bits each for x and y  (max 16k per axis)
			public float fValue;     // value in top left corner
			public int nLeftVertex;  // vertex on left edge
			public int nTopVertex;   // vertex on top edge
			public bool bTouched;   // true if node has been seen

			public void Initialize(uint x, uint y) {
				X = x;
				Y = y;
				fValue = _fValueSentinel;
				nLeftVertex = nTopVertex = -1;
				bTouched = false;
			}

			public uint X
			{
				get => _nPosition & 0xFFFF;
				set => _nPosition = (Y << 16) | (value & 0xFFFF);
			}

			public uint Y
			{
				get => (_nPosition >> 16) & 0xFFFF;
				set => _nPosition = ((value & 0xFFFF) << 16) | X;
			}
		}

		Cell[][] _cells;


		struct SeedPoint
		{
			public float x;
			public float y;
			public SeedPoint(float fX, float fY) { x = fX; y = fY; }
		}

		readonly ArrayList _seedPoints;

		ImplicitField2d _field;
		readonly ArrayList _cellStack;
		readonly bool[] _bEdgeSigns;


		public MarchingQuads(int nSubdivisions, AxisAlignedBox2f bounds, float fIsoValue) {
			Stroke = new DPolyLine2f();
			_bounds = new AxisAlignedBox2f();

			_nCells = nSubdivisions;
			SetBounds(bounds);

			_cells = null;
			InitializeCells();

			_seedPoints = new ArrayList();
			_cellStack = new ArrayList();

			_bEdgeSigns = new bool[4];

			_fIsoValue = fIsoValue;
		}

		public int Subdivisions
		{
			get => _nCells;
			set { _nCells = value; SetBounds(_bounds); InitializeCells(); }
		}

		public AxisAlignedBox2f Bounds
		{
			get => _bounds;
			set => SetBounds(value);
		}

		public DPolyLine2f Stroke { get; }


		public AxisAlignedBox2f GetBounds() {
			return _bounds;
		}


		public void AddSeedPoint(float x, float y) {
			_seedPoints.Add(new SeedPoint(x - _fXShift, y - _fYShift));
		}

		public void ClearSeedPoints() {
			_seedPoints.Clear();
		}

		public void ClearStroke() {
			Stroke.Clear();
		}

		public void Polygonize(ImplicitField2d field) {

			_field = field;

			ResetCells();  // reset bTouched flags

			_cellStack.Clear();

			// iterate over seed points
			for (var i = 0; i < _seedPoints.Count; ++i) {

				var p = (SeedPoint)_seedPoints[i];
				var xi = (int)(p.x / _fCellSize);
				var yi = (int)(p.y / _fCellSize);

				var bFoundSurface = false;
				while (!bFoundSurface && yi > 0 && yi < _cells.Length - 1 && xi > 0 && xi < _cells[0].Length - 1) {

					if (_cells[yi][xi].bTouched == false) {
						var bResult = ProcessCell(xi, yi);
						if (bResult == true) {
							bFoundSurface = true;
						}
					}
					else {
						bFoundSurface = true;
					}

					xi--;
				}

				while (_cellStack.Count != 0) {
					var cell = (Cell)_cellStack[_cellStack.Count - 1];
					_cellStack.RemoveAt(_cellStack.Count - 1);

					if (_cells[(int)cell.Y][(int)cell.X].bTouched == false) {
						ProcessCell((int)cell.X, (int)cell.Y);
					}
				}
			}

		}


		void SubdivideStep(ref float fValue1, ref float fValue2, ref float fX1, ref float fY1, ref float fX2, ref float fY2,
						bool bVerticalEdge) {

			var fAlpha = 0.5f;

			float fX;

			float fY;
			if (bVerticalEdge) {
				fX = fX1;
				fY = (fAlpha * fY1) + ((1.0f - fAlpha) * fY2);
			}
			else {
				fX = (fAlpha * fX1) + ((1.0f - fAlpha) * fX2);
				fY = fY1;
			}

			var fValue = (float)_field.Value(fX, fY);
			if (fValue < _fIsoValue) {
				fValue1 = fValue;
				fX1 = fX;
				fY1 = fY;
			}
			else {
				fValue2 = fValue;
				fX2 = fX;
				fY2 = fY;
			}

		}

		int LerpAndAddStrokeVertex(float fValue1, float fValue2, int x1, int y1, int x2, int y2, bool bVerticalEdge) {

			// swap if need be
			if (fValue1 > fValue2) {
				(x2, x1) = (x1, x2);
				(y2, y1) = (y1, y2);
				(fValue2, fValue1) = (fValue1, fValue2);
			}

			var fRefValue1 = fValue1;
			var fRefValue2 = fValue2;
			var fX1 = (x1 * _fCellSize) + _fXShift;
			var fY1 = (y1 * _fCellSize) + _fYShift;
			var fX2 = (x2 * _fCellSize) + _fXShift;
			var fY2 = (y2 * _fCellSize) + _fYShift;

			for (var i = 0; i < 10; ++i) {
				SubdivideStep(ref fRefValue1, ref fRefValue2, ref fX1, ref fY1, ref fX2, ref fY2, bVerticalEdge);
			}

			return Math.Abs(fRefValue1) < Math.Abs(fRefValue2) ? Stroke.AddVertex(fX1, fY1) : Stroke.AddVertex(fX2, fY2);

		}


		int GetLeftEdgeVertex(int xi, int yi) {

			var cell = _cells[yi][xi];
			if (cell.nLeftVertex != -1) {
				return cell.nLeftVertex;
			}

			_cells[yi][xi].nLeftVertex = LerpAndAddStrokeVertex(cell.fValue, _cells[yi + 1][xi].fValue,
				xi, yi, xi, yi + 1, true);
			return _cells[yi][xi].nLeftVertex;
		}

		int GetRightEdgeVertex(int xi, int yi) {

			var cell = _cells[yi][xi + 1];
			if (cell.nLeftVertex != -1) {
				return cell.nLeftVertex;
			}

			_cells[yi][xi + 1].nLeftVertex = LerpAndAddStrokeVertex(cell.fValue, _cells[yi + 1][xi + 1].fValue,
				xi + 1, yi, xi + 1, yi + 1, true);
			return _cells[yi][xi + 1].nLeftVertex;
		}

		int GetTopEdgeVertex(int xi, int yi) {

			var cell = _cells[yi][xi];
			if (cell.nTopVertex != -1) {
				return cell.nTopVertex;
			}

			_cells[yi][xi].nTopVertex = LerpAndAddStrokeVertex(cell.fValue, _cells[yi][xi + 1].fValue,
				xi, yi, xi + 1, yi, false);
			return _cells[yi][xi].nTopVertex;
		}

		int GetBottomEdgeVertex(int xi, int yi) {

			var cell = _cells[yi + 1][xi];
			if (cell.nTopVertex != -1) {
				return cell.nTopVertex;
			}

			_cells[yi + 1][xi].nTopVertex = LerpAndAddStrokeVertex(cell.fValue, _cells[yi + 1][xi + 1].fValue,
				xi, yi + 1, xi + 1, yi + 1, false);
			return _cells[yi + 1][xi].nTopVertex;
		}

		bool ProcessCell(int xi, int yi) {

			_cells[yi][xi].bTouched = true;

			var nCase = 0;
			for (var i = 0; i < 4; ++i) {
				var nxi = xi + (i & 1);
				var nyi = yi + ((i >> 1) & 1);
				if (_cells[nyi][nxi].fValue == _fValueSentinel) {
					_cells[nyi][nxi].fValue = _field.Value((nxi * _fCellSize) + _fXShift, (nyi * _fCellSize) + _fYShift);
				}

				_bEdgeSigns[i] = _cells[nyi][nxi].fValue > _fIsoValue;
				nCase |= (_bEdgeSigns[i] == true ? 1 : 0) << i;

			}

			if (nCase is 0 or 15) {
				return false;       // nothing to do - inside or outside...
			}


			// don't actually need to compute all of these...
			int nLeftV = 0, nRightV = 0, nTopV = 0, nBottomV = 0;
			if (_bEdgeSigns[0] != _bEdgeSigns[2]) {
				nLeftV = GetLeftEdgeVertex(xi, yi);
			}

			if (_bEdgeSigns[1] != _bEdgeSigns[3]) {
				nRightV = GetRightEdgeVertex(xi, yi);
			}

			if (_bEdgeSigns[0] != _bEdgeSigns[1]) {
				nTopV = GetTopEdgeVertex(xi, yi);
			}

			if (_bEdgeSigns[2] != _bEdgeSigns[3]) {
				nBottomV = GetBottomEdgeVertex(xi, yi);
			}

			// evaluate "middle" decider case...
			var fDecider = 0.0f;
			if (nCase is 6 or 9) {
				fDecider = _field.Value((xi * _fCellSize) + (_fCellSize / 2.0f) + _fXShift,
					(yi * _fCellSize) + (_fCellSize / 2.0f) + _fYShift);
			}

			var nSidesToPush = 0;

			switch (nCase) {
				case 1:
				case 14:
					Stroke.AddEdge(nLeftV, nTopV);
					nSidesToPush = LEFT | TOP;
					break;
				case 2:
				case 13:
					Stroke.AddEdge(nTopV, nRightV);
					nSidesToPush = RIGHT | TOP;
					break;
				case 4:
				case 11:
					Stroke.AddEdge(nBottomV, nLeftV);
					nSidesToPush = LEFT | BOTTOM;
					break;
				case 7:
				case 8:
					Stroke.AddEdge(nRightV, nBottomV);
					nSidesToPush = RIGHT | BOTTOM;
					break;

				case 3:
				case 12:
					Stroke.AddEdge(nRightV, nLeftV);
					nSidesToPush = LEFT | RIGHT;
					break;
				case 5:
				case 10:
					Stroke.AddEdge(nTopV, nBottomV);
					nSidesToPush = BOTTOM | TOP;
					break;

				case 9:
					if (fDecider > _fIsoValue) {
						Stroke.AddEdge(nLeftV, nBottomV);
						Stroke.AddEdge(nTopV, nRightV);
					}
					else {
						Stroke.AddEdge(nLeftV, nTopV);
						Stroke.AddEdge(nBottomV, nRightV);
					}
					nSidesToPush = ALL;
					break;

				case 6:
					if (fDecider > _fIsoValue) {
						Stroke.AddEdge(nLeftV, nTopV);
						Stroke.AddEdge(nBottomV, nRightV);
					}
					else {
						Stroke.AddEdge(nLeftV, nBottomV);
						Stroke.AddEdge(nTopV, nRightV);
					}
					nSidesToPush = ALL;
					break;
			}


			// ?!??!?! WHY ARE TOP AND BOTTOM REVERSED ????!?!?!?! 
			// because the "Top" edge is the "y" edge, and the "Bottom" edge is the "y+1" edge.
			// So when we want to push the quad "below" the "Bottom" edge, that (y+1), and
			// the one "Above" the top edge is (y-1). Maybe rename?

			if ((nSidesToPush & LEFT) != 0 && xi - 1 >= 0 && _cells[yi][xi - 1].bTouched == false) {
				_cellStack.Add(_cells[yi][xi - 1]);
			}

			if ((nSidesToPush & RIGHT) != 0 && xi + 1 < _nCells && _cells[yi][xi + 1].bTouched == false) {
				_cellStack.Add(_cells[yi][xi + 1]);
			}

			if ((nSidesToPush & BOTTOM) != 0 && yi + 1 < _nCells && _cells[yi + 1][xi].bTouched == false) {
				_cellStack.Add(_cells[yi + 1][xi]);
			}

			if ((nSidesToPush & TOP) != 0 && yi - 1 >= 0 && _cells[yi - 1][xi].bTouched == false) {
				_cellStack.Add(_cells[yi - 1][xi]);
			}

			return true;

		}


		// private members

		void ResetCells() {
			for (uint y = 0; y < _cells.Length; ++y) {
				for (uint x = 0; x < _cells.Length; ++x) {
					_cells[y][x].bTouched = false;
					_cells[y][x].nLeftVertex = _cells[y][x].nTopVertex = -1;
				}
			}
		}

		void InitializeCells() {

			_cells = new Cell[_nCells + 1][];
			for (uint y = 0; y < _cells.Length; ++y) {
				_cells[y] = new Cell[_nCells + 1];
				for (uint x = 0; x < _cells.Length; ++x) {
					_cells[y][x].Initialize(x, y);
				}
			}
		}

		void SetBounds(AxisAlignedBox2f bounds) {
			_bounds = bounds;

			_fXShift = (bounds.Min.x < 0) ? bounds.Min.x : -bounds.Min.x;
			_fYShift = (bounds.Min.y < 0) ? bounds.Min.y : -bounds.Min.y;

			_fScale = (bounds.Width > bounds.Height) ? bounds.Width : bounds.Height;

			_fCellSize = _fScale / _nCells;
		}


	}
}
