using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Newtonsoft.Json.Linq;

namespace RNumerics
{
	public static class StructEditor
	{
		public static Type GetFielType(Type targetType, string fieldName) {
			return GetFielTypeHelper(targetType, fieldName);
		}

		private static (string field, int arrayPos)? GetArrayIndex(string fieldString) {
			if (fieldString.Contains('[') & fieldString.Contains(']')) {
				var start = fieldString.IndexOf('[');
				var end = fieldString.IndexOf(']');
				var field = fieldString.Substring(0, start);
				try {
					var arrayString = fieldString.Substring(start + 1, end - start - 1);
					var arrayIndex = int.Parse(arrayString);
					return (field, arrayIndex);
				}
				catch {
					return (field, 0);
				}
			}
			else {
				return null;
			}
		}

		private static Type GetFielTypeHelper(Type structType, string fieldName) {
			var dotIndex = fieldName.IndexOf('.');
			if (dotIndex >= 0) {
				// Extract the current field name and remaining field name parts
				var currentFieldName = fieldName.Substring(0, dotIndex);
				var remainingFieldName = fieldName.Substring(dotIndex + 1);

				var array = GetArrayIndex(currentFieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(currentFieldName);

					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}

					// Recursively get the value of the next field in the hierarchy
					return GetFielTypeHelper(fieldInfo.FieldType, remainingFieldName);
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}
						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{array.Value.field} is not a field of type {structType.Name}");
						}

						return GetFielTypeHelper(fieldInfo.FieldType.GetElementType(), remainingFieldName);
					}
					else {
						if (!structType.IsArray) {
							throw new ArgumentException($"{array.Value.field} is not array");
						}
						// Return the value of the field
						return GetFielTypeHelper(structType.GetElementType(), remainingFieldName);
					}
				}
			}
			else {
				var array = GetArrayIndex(fieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(fieldName);
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}
					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}

					// Return the value of the field
					return fieldInfo.FieldType;
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}

						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{array.Value.field} is not a field of type {structType.Name}");
						}

						if (!fieldInfo.FieldType.IsArray) {
							throw new ArgumentException($"{array.Value.field} is not array");
						}

						// Return the value of the field
						return fieldInfo.FieldType.GetElementType();
					}
					else {
						if (!structType.IsArray) {
							throw new ArgumentException($"{array.Value.field} is not array");
						}
						// Return the value of the field
						return structType.GetElementType();
					}

				}
			}
		}


		public static object GetFieldValue(object targetStruct, string fieldName) {
			var structType = targetStruct.GetType();
			var targetStructBoxed = targetStruct;
			return GetFieldValueHelper(targetStructBoxed, structType, fieldName);
		}

		private static object GetFieldValueHelper(object targetStructBoxed, Type structType, string fieldName) {
			var dotIndex = fieldName.IndexOf('.');
			if (dotIndex >= 0) {
				// Extract the current field name and remaining field name parts
				var currentFieldName = fieldName.Substring(0, dotIndex);
				var remainingFieldName = fieldName.Substring(dotIndex + 1);
				var array = GetArrayIndex(fieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(currentFieldName);

					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}

					// Get the value of the current field
					var fieldValue = fieldInfo.GetValue(targetStructBoxed);

					// Recursively get the value of the next field in the hierarchy
					return GetFieldValueHelper(fieldValue, fieldInfo.FieldType, remainingFieldName);
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);

						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
						}
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}

						// Get the value of the current field
						var fieldValue = fieldInfo.GetValue(targetStructBoxed);
						if (fieldValue is IList list) {
							var nest = list[array.Value.arrayPos];
							return GetFieldValueHelper(nest, nest.GetType(), remainingFieldName);
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
					else {
						if (targetStructBoxed is IList list) {
							var nest = list[array.Value.arrayPos];
							return GetFieldValueHelper(nest, nest.GetType(), remainingFieldName);
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
				}
			}
			else {
				var array = GetArrayIndex(fieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(fieldName);
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}
					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}

					// Return the value of the field
					return fieldInfo.GetValue(targetStructBoxed);
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);

						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{array.Value.field} is not a field of type {structType.Name}");
						}
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}

						var startingArray = fieldInfo.GetValue(targetStructBoxed);
						if (startingArray is IList list) {
							return list[array.Value.arrayPos];
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
					else {
						if (targetStructBoxed is IList list) {
							return list[array.Value.arrayPos];
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
				}
			}
		}

		public static void SetFieldValueObject(object targetStructBoxed, string fieldName, object value) {
			var structType = targetStructBoxed.GetType();
			SetFieldValueHelper(targetStructBoxed, structType, fieldName, value);
		}

		public static void SetFieldValue<T>(ref T targetStruct, string fieldName, object value) {
			var structType = targetStruct.GetType();
			var targetStructBoxed = (object)targetStruct;
			SetFieldValueHelper(targetStructBoxed, structType, fieldName, value);
			targetStruct = (T)targetStructBoxed;
		}

		private static void SetFieldValueHelper(object targetStructBoxed, Type structType, string fieldName, object value) {
			var dotIndex = fieldName.IndexOf('.');
			if (dotIndex >= 0) {

				// Extract the current field name and remaining field name parts
				var currentFieldName = fieldName.Substring(0, dotIndex);
				var remainingFieldName = fieldName.Substring(dotIndex + 1);
				var array = GetArrayIndex(currentFieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(currentFieldName);
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}
					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}

					// Get the value of the current field
					var fieldValue = fieldInfo.GetValue(targetStructBoxed);

					// Recursively set the value of the next field in the hierarchy
					SetFieldValueHelper(fieldValue, fieldInfo.FieldType, remainingFieldName, value);

					// Set the value of the current field
					fieldInfo.SetValue(targetStructBoxed, fieldValue);
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}
						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{array.Value.field} is not a field of type {structType.Name}");
						}

						// Get the value of the current field
						var fieldValue = fieldInfo.GetValue(targetStructBoxed);

						if (fieldValue is IList list) {
							var data = list[array.Value.arrayPos];
							SetFieldValueHelper(data, list[array.Value.arrayPos].GetType(), remainingFieldName, value);
							list[array.Value.arrayPos] = data;
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}

						// Set the value of the current field
						fieldInfo.SetValue(targetStructBoxed, fieldValue);


					}
					else {
						if (targetStructBoxed is IList list) {
							var data = list[array.Value.arrayPos];
							SetFieldValueHelper(data, list[array.Value.arrayPos].GetType(), remainingFieldName, value);
							list[array.Value.arrayPos] = data;
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
				}
			}
			else {
				var array = GetArrayIndex(fieldName);
				if (array is null) {
					// Get the field info for the current field
					var fieldInfo = structType.GetField(fieldName);

					// Check if the field exists
					if (fieldInfo == null) {
						throw new ArgumentException($"{fieldName} is not a field of type {structType.Name}");
					}
					if (!fieldInfo.IsPublic) {
						throw new Exception("Not Public Field");
					}

					// Check if the value is of the correct type
					if (!fieldInfo.FieldType.IsAssignableFrom(value.GetType())) {
						// Attempt to convert the value to the correct type
						value = Convert.ChangeType(value, fieldInfo.FieldType);
					}

					// Set the value of the field
					fieldInfo.SetValue(targetStructBoxed, value);
				}
				else {
					if (!string.IsNullOrEmpty(array.Value.field)) {
						// Get the field info for the current field
						var fieldInfo = structType.GetField(array.Value.field);

						// Check if the field exists
						if (fieldInfo == null) {
							throw new ArgumentException($"{array.Value.field} is not a field of type {structType.Name}");
						}
						if (!fieldInfo.IsPublic) {
							throw new Exception("Not Public Field");
						}

						var startingArray = fieldInfo.GetValue(targetStructBoxed);
						if (startingArray is IList list) {
							list[array.Value.arrayPos] = value;
							// Set the value of the field
							fieldInfo.SetValue(targetStructBoxed, startingArray);
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
					else {
						if (targetStructBoxed is IList list) {
							list[array.Value.arrayPos] = value;
						}
						else {
							throw new ArgumentException($"{array.Value.field} is not a array type");
						}
					}
				}
			}
		}

	}

}
