using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace RhuEngine.Components
{
	public interface IListAttacher : IComponent
	{
		public void SetTarget(ISyncList target);
	}

	public sealed class CategoryData<T>
	{
		public CategoryData<T> Parrent;
		public string CurrentPath;
		public string FullPath;
		public int pathIndex = 0;
		public Type[] currentTypes = Array.Empty<Type>();

		public CategoryData<T>[] children = Array.Empty<CategoryData<T>>();

		public CategoryData(string path = "/", CategoryData<T> parrent = null) {
			CurrentPath = path;
			Parrent = parrent;
			FullPath = (parrent?.FullPath ?? string.Empty) + "/" + path;
			FullPath = FullPath.Replace("//", "/");
			if (FullPath == "/" & parrent is null) {
				LoadTypes();
			}
		}

		public void LoadTypes() {
			var allTypes = (from a in AppDomain.CurrentDomain.GetAssemblies()
							from type in a.GetTypes()
							where type.IsAssignableTo(typeof(T))
							where !type.IsAbstract & type.IsClass
							where type.GetCustomAttribute<HideCategoryAttribute>() is null
							let at = type.GetCustomAttribute<CategoryAttribute>()
							where (type.GetCustomAttribute<PrivateSpaceOnlyAttribute>() is null) & (type.GetCustomAttribute<OverlayOnlyAttribute>() is null)
							orderby (at?.Paths.Length ?? 0) ascending
							select (at, type)).ToArray();
			LoadSubTypes(allTypes);
		}

		public void LoadSubTypes((CategoryAttribute at, Type type)[] allTypes) {
			var subData = (from typeData in allTypes
						   where ((typeData.at?.Paths.Length ?? 0) == pathIndex) | ((typeData.at?.Paths.Length ?? 0) == pathIndex + 1)
						   where typeData.at?.IsParrentPath(FullPath) ?? (FullPath == "/")
						   select typeData).ToArray();
			currentTypes = (from typeData in subData
							where (typeData.at?.Paths.Length ?? 0) == pathIndex
							select typeData.type).ToArray();
			List<CategoryData<T>> categoryDatas = new();
			foreach (var groupRelation in from typeData in subData
										  where (typeData.at?.Paths.Length ?? 0) == (pathIndex + 1)
										  group typeData by typeData.at.FullPath) {
				var newCat = new CategoryData<T>(groupRelation.First().at.Paths.Last(), this) {
					pathIndex = pathIndex + 1
				};
				categoryDatas.Add(newCat);
				newCat.LoadSubTypes(allTypes);
			}
			children = categoryDatas.ToArray();
		}
	}


	[Category(new string[] { "Developer/Inspectors" })]
	public sealed class ListAttacher<T> : Component, IListAttacher where T : ISyncObject
	{
		public readonly SyncRef<IAbstractObjList<T>> TargetAddingObject;
		public readonly SyncRef<BoxContainer> MainBox;
		public Type SubType => typeof(T);

		public bool IsAbstract => SubType.IsAbstract | SubType.IsInterface;

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!IsAbstract) {
				throw new Exception("Type is not vailed None Vailed");
			}

		}

		public static CategoryData<T> categoryData = new();

		protected override void OnAttach() {
			base.OnAttach();
			var scrollBox = Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			MainBox.Target = scrollBox.Entity.AddChild("MainBox").AttachComponent<BoxContainer>();
			MainBox.Target.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			MainBox.Target.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			MainBox.Target.Vertical.Value = true;
			LoadUI("/");
		}

		private Button AddButton(string buttonName, Colorf tint) {
			var button = MainBox.Target.Entity.AddChild(buttonName).AttachComponent<Button>();
			button.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			button.ModulateSelf.Value = tint;
			button.Text.Value = buttonName;
			button.Alignment.Value = RButtonAlignment.Center;
			return button;
		}

		private AddSingleValuePram<string> AddButtonStringPram(string buttonName, Colorf tint, string pram) {
			var button = MainBox.Target.Entity.AddChild(buttonName).AttachComponent<Button>();
			button.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			button.ModulateSelf.Value = tint;
			button.Text.Value = buttonName;
			var pramAdder = button.Entity.AttachComponent<AddSingleValuePram<string>>();
			pramAdder.Value.Value = pram;
			button.Alignment.Value = RButtonAlignment.Center;
			button.Pressed.Target = pramAdder.Call;
			return pramAdder;
		}

		public void LoadUI(string path) {
			if (MainBox.Target is null) {
				return;
			}
			MainBox.Target.Entity.DestroyChildren();
			if (path == "/") {
				AddButton("Close", Colorf.Red).Pressed.Target = Close;
			}
			else {
				if (path.EndsWith('/') & (path.Length > 1)) {
					path = path.Remove(path.Length - 1);
				}
				AddButtonStringPram("Back", Colorf.Red, path.Remove(path.LastIndexOf('/') + 1)).Target.Target = NavToPath;
			}
			var pathData = path.Split('/');
			var targetCat = categoryData;
			foreach (var item in pathData) {
				foreach (var chil in targetCat.children) {
					if (chil.CurrentPath == item) {
						targetCat = chil;
						break;
					}
				}
			}
			foreach (var item in targetCat.children) {
				AddButtonStringPram(item.CurrentPath, Colorf.RhubarbGreen, item.FullPath).Target.Target = NavToPath;
			}
			foreach (var item in targetCat.currentTypes) {
				if (item.IsGenericType) {
					AddButtonStringPram(item.GetFormattedName(), Colorf.DarkBlue, item.FullName).Target.Target = AddData;
				}
				else {
					AddButtonStringPram(item.GetFormattedName(), Colorf.Blue, item.FullName).Target.Target = AddData;
				}
			}
		}

		[Exposed]
		public void NavToPath(string targrtPath) {
			LoadUI(targrtPath);
		}

		[Exposed]
		public void AddData(string type) {
			if (TargetAddingObject.Target is null) {
				return;
			}
			var t = Type.GetType(type);
			if (t is null) {
				Close();
				return;
			}
			if (t.IsGenericType & !t.IsGenericTypeDefinition) {
				//Build looking for generic UI
				Close(); //Timp
				return;
			}
			if (!t.IsAssignableTo(typeof(T))) {
				return;
			}
			try {
				if (typeof(T) == typeof(IComponent)) {
					if (TargetAddingObject.Target.Parent is Entity entity) {
						entity.AttachComponent(t);
					}
					else {
						throw new NotSupportedException();
					}
				}
				else {
					TargetAddingObject.Target.Add(t);
				}
			}
			catch(Exception e) {
				RLog.Err($"Error when list attaching on type {t} Error: {e}");
			}
			finally {
				Close();
			}
		}

		[Exposed]
		public void Close() {
			Entity.Destroy();
		}

		public void SetTarget(ISyncList target) {
			if (target is IAbstractObjList<T> currentObject) {
				TargetAddingObject.Target = currentObject;
			}
		}
	}
}