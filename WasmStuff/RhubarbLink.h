#if !defined(RhuLink)
#define RhuLink

#if defined __STDC_VERSION__ && __STDC_VERSION__ > 201710L
/* bool, true and false are keywords.  */
#else
#define bool	_Bool
#define true	1
#define false	0
#endif

#define byte unsigned char
#define sbyte char
#define ushort unsigned short
#define uint unsigned int
#define ulong unsigned long
#define char_16 short



#define RhuVariant unsigned long
#define NULLRhuID -1
#define RhuID int
#define RhuString RhuID
#define RhuDelegate RhuID
#define RhuType RhuID

// READ THIS
// Note rhubarb does not clean up data that your using automatically so after creating a string you need to remove ref to clean it up
// This header file should be used with a code generator to find the methods look at rhubarb exports json for info

// This will tell Rhubarb to remove a object from the referencing list so it can be cleaned up by the garbage collector
// Tells Rhubarb To cleanup object ref
extern void remove_ref(RhuID targetObject);

// Makes a string that can then be used with methods with ascii
extern RhuString create_ascii_string(char* data);

// Makes a string that can then be used with methods with utf8
extern RhuString create_UTF8_string(char* data);

// Makes a string that can then be used with methods with utf32
extern RhuString create_UTF32_string(int* data);

// Returns the length of unwritten string
extern int to_native(RhuString targetString, char* targetAdress, int size);

#if defined(__cplusplus)
// Gets a Delegate that can be used to then call a method
extern RhuDelegate method_bind(char* targetType, char* methodName, int argumentCount, int methodHash, char** genericDefinition = nullptr, int genericDefinitionAmount = 0);
#else
// Gets a Delegate that can be used to then call a method
extern RhuDelegate method_bind(char* targetType, char* methodName, int argumentCount, int methodHash, char** genericDefinition, int genericDefinitionAmount);
#endif

// Gets constructor for structs like vector3 and other data types
extern RhuDelegate constructor_bind(char* targetType, int argumentCount, int methodHash);

// Gets a Delegate that takes target object and returns data in that field
extern RhuDelegate field_bind(char* targetType, char* fieldName);

// Gets a Delegate to the get method on a property
extern RhuDelegate property_get_bind(char* targetType, char* propertyName);

// Gets a Delegate to the set method on a property
extern RhuDelegate property_set_bind(char* targetType, char* propertyName);

// Calls a Delegate with arguments and return Data
// Argument Info
// First Argument is the target Object unless it is a static function
// int, uint, float, byte, bool, sbyte, short, ushort, char - use the int value
// long, ulong, double - use two int values and skip an index
// for all RhuIDs just put them in
extern RhuVariant call_method(RhuDelegate targetDelegate, RhuVariant* arguments);

// Gets what type an object is
extern RhuType get_type(RhuID targetObject);

// Calls ToString method on the object
extern RhuString to_string(RhuID targetObject);

// Gets type using rhubarbs type parser
extern RhuType parse_type_utf8(char* string);

// Gets type using rhubarbs type parser
extern RhuType parse_type(RhuString typeString);

// Returns the object of what runner component the script is running on
extern RhuID get_runner();

extern int array_length(RhuID targetArray);

extern RhuVariant array_get_value(RhuID targetArray, int index);

extern int string_length(RhuString targetString);

extern RhuString string_append(RhuString targetString_a, RhuString targetString_b);

extern bool string_equal(RhuString targetString_a, RhuString targetString_b);

extern char_16 string_get_char(RhuString targetString, int index);

// RhuVariant Helpers
// variant to
inline bool variant_to_bool(RhuVariant x) {
	bool value = false;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline byte variant_to_byte(RhuVariant x) {
	byte value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline sbyte variant_to_sbyte(RhuVariant x) {
	sbyte value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline char_16 variant_to_char(RhuVariant x) {
	char_16 value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline int variant_to_int(RhuVariant x) {
	int value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline uint variant_to_uint(RhuVariant x) {
	uint value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline long variant_to_long(RhuVariant x) {
	long value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline float variant_to_float(RhuVariant x) {
	float value = 0.0f;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline double variant_to_double(RhuVariant x) {
	double value = 0.0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline ulong variant_to_ulong(RhuVariant x) {
	return x;
}

inline RhuID variant_to_rhu_id(RhuVariant x) {
	RhuID value = NULLRhuID;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

// datatype to variant
inline RhuVariant bool_to_variant(bool x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant byte_to_variant(byte x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant sbyte_to_variant(sbyte x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant char_to_variant(char_16 x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant int_to_variant(int x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant uint_to_variant(uint x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant long_to_variant(long x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}


inline RhuVariant float_to_variant(float x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant double_to_variant(double x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

inline RhuVariant ulong_to_variant(ulong x) {
	return x;
}

inline RhuVariant rhu_id_to_variant(RhuID x) {
	RhuVariant value = 0;
	memcpy(&value, &x, sizeof(value) < sizeof(x) ? sizeof(value) : sizeof(x));
	return value;
}

#endif
