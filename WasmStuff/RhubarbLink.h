#if !defined(RhubarbLink)

#define RhuLink

#define NULLRhuID -1
#define RhuID int
#define RhuString RhuID
#define RhuDelegate RhuID

// READ THIS
// Note rhubarb does not clean up data that your using automatically so after creating a string you need to remove ref to clean it up
// This header file should be used with a code generator to find the methods look at rhubarb exports json for info

//Tells Rhubarb To cleanup object ref
extern void remove_ref(RhuID targetObject);

//Makes a string that can then be used with methods with ascii
extern RhuString create_ascii_string(char* data);

//Makes a string that can then be used with methods with utf8
extern RhuString create_UTF8_string(char* data);

//Makes a string that can then be used with methods with utf32
extern RhuString create_UTF32_string(int* data);

#if defined(__cplusplus)
//Gets a Delegate that can be used to then call a method
extern RhuDelegate method_bind(RhuString targetType, RhuString methodName, int argumentCount, int methodHash, RhuString genericDefinition = NULLRhuID);
#else
//Gets a Delegate that can be used to then call a method
extern RhuDelegate method_bind(RhuString targetType, RhuString methodName, int argumentCount, int methodHash, RhuString genericDefinition);
#endif

//Gets a Delegate that takes target object and returns data in that field
extern RhuDelegate field_bind(RhuString targetType, RhuString fieldName);

//Gets a Delegate to the get method on a property
extern RhuDelegate property_get_bind(RhuString targetType, RhuString fieldName);

//Gets a Delegate to the set method on a property
extern RhuDelegate property_set_bind(RhuString targetType, RhuString fieldName);

//Calls a Delegate with arguments and return Data
// Argument Info
// int, uint, float, byte, bool, sbyte, short, ushort, char - use the int value
// long, ulong, double - use two int values and skip an index
// for all RhuIDs just put them in
extern long call_method(RhuDelegate targetDelegate, int** arguments);

#endif
