using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RNumerics
{
	public static class StructEditor
	{
		public static Type GetFielType(Type targetType, string fieldName) {
			return GetFielTypeHelper(targetType, fieldName);
		}

		private static Type GetFielTypeHelper(Type structType, string fieldName) {
			var dotIndex = fieldName.IndexOf('.');
			if (dotIndex >= 0) {
				// Extract the current field name and remaining field name parts
				var currentFieldName = fieldName.Substring(0, dotIndex);
				var remainingFieldName = fieldName.Substring(dotIndex + 1);

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
				// This is the last field in the hierarchy, return the value

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
				// This is the last field in the hierarchy, return the value

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
				// This is the last field in the hierarchy, set the value

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
		}

	}

}
