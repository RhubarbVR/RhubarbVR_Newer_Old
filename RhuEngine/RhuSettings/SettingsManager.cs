using Newtonsoft.Json.Linq;

using System;
using System.Linq;
using System.Reflection;

namespace RhuSettings
{
	public static class SettingsManager
	{
		public static DataList GetDataFromJson(string json) {
			var obj = JObject.Parse(json);
			return LoadObject(obj);
		}

		public static DataList LoadObject(JObject obj) {
			var value = new DataList();
			foreach (var item in obj) {
				DataObject val;
				switch (item.Value.Type) {
					case JTokenType.Object:
						val = LoadObject(item.Value.ToObject<JObject>());
						break;
					default:
						var node = new DataNode();
						node.Setval(item.Value.ToObject<object>());
						val = node;
						break;
				}
				value.AddDataObject(item.Key, val);
			}
			return value;
		}

		public static string GetJsonFromData(DataList val) {
			var jobj = GetJsonFromDataList(val);
			return jobj.ToString();
		}

		public static JObject GetJsonFromDataList(DataList val) {
			var obj = new JObject();
			foreach (var item in val) {
				if (item.Value.GetType() == typeof(DataList)) {
					obj[item.Key] = GetJsonFromDataList((DataList)item.Value);
				}
				else {
					var value = ((DataNode)item.Value)?.Getval();
					if (value is string[][] data) {
						obj[item.Key] = new JArray(data.Select(x=>new JArray(x)).ToArray());
					}
					else {
						obj[item.Key] = new JValue(value);
					}
				}
			}
			return obj;
		}
		public static T LoadSettingsObject<T>(T start, params DataList[] args) where T : SettingsObject {
			foreach (var item in args) {
				start = (T)LoadSettingsObjectInternal(start, item);
			}
			return start;
		}

		public static T LoadSettingsObject<T>(params DataList[] args) where T : SettingsObject, new() {
			var val = new T();
			foreach (var item in args) {
				val = (T)LoadSettingsObjectInternal(val, item);
			}
			return val;
		}

		public static SettingsObject LoadSettingsObjectInternal(SettingsObject obj, DataList startList) {
			var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields) {
				var argfield = field.GetCustomAttribute<SettingsField>();
				if (argfield != null) {
					var value = field.GetValue(obj);
					var help = argfield.help;
					var Path = argfield.Path.ToString();
					var pathparts = Path.Split('/');
					var fieldname = field.Name;
					var dataType = field.FieldType;
					DataList location;
					if (Path == "/") {
						location = startList;
					}
					else {
						var pos = location = startList;
						foreach (var item in pathparts) {
							if (item != "") {
								location = pos = pos.AddList(item);
							}
						}
					}
					if (typeof(SettingsObject).IsAssignableFrom(dataType)) {
						var setobj = location.AddList(fieldname);
						var val = value != null ? (SettingsObject)value : (SettingsObject)Activator.CreateInstance(dataType);
						LoadSettingsObjectInternal(val, setobj);
						field.SetValue(obj, val);
					}
					else {
						var val = (DataNode)location.GetDataObject(fieldname);
						if (val != null) {
							if (dataType.IsEnum) {
								try {
									field.SetValue(obj, Enum.Parse(dataType, (string)val.Getval()));
								}
								catch { }
							}
							else {
								field.SetValue(obj, ChangeType(val.Getval(), dataType));
							}
						}
					}
				}
			}
			return obj;
		}
		public static object ChangeType(object source, Type dest) {
			if(dest == typeof(string[][])) {
				if(source is JArray jArray) {
					return jArray.Select(x => ((JArray)x).Select(x=>(string)x).ToArray()).ToArray();
				}
			}
			return Convert.ChangeType(source, dest);
		}
		public static DataList GetDataListFromSettingsObject(SettingsObject obj, DataList startlist) {
			var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields) {
				var argfield = field.GetCustomAttribute<SettingsField>();
				if (argfield != null) {
					var value = field.GetValue(obj);
					var help = argfield.help;
					var Path = argfield.Path.ToString();
					var pathparts = Path.Split('/');
					var fieldname = field.Name;
					var dataType = field.FieldType;
					DataList location;
					if (Path == "/") {
						location = startlist;
					}
					else {
						location = startlist;
						foreach (var item in pathparts) {
							if (item != "") {
								location = location.AddList(item);
							}
						}
					}
					if (typeof(SettingsObject).IsAssignableFrom(dataType)) {
						var setobj = location.AddList(fieldname);
						if (value == null) {
							value = (SettingsObject)Activator.CreateInstance(dataType);
							field.SetValue(obj, value);
						}
						GetDataListFromSettingsObject((SettingsObject)value, setobj);

					}
					else {
						var val = new DataNode();
						if (field.FieldType.IsEnum) {
							val.Setval(Enum.GetName(field.FieldType, field.GetValue(obj)));
						}
						else {
							val.Setval(field.GetValue(obj));
						}
						location.AddDataObject(fieldname, val);
					}
				}
			}
			return startlist;
		}

	}
}
