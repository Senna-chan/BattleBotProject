#ifndef COMMON_HELPERS_H
#define COMMON_HELPERS_H

char *trimwhitespace(char *str);
static int jsoneq(const char *json, jsmntok_t *tok, const char *s);
int StringStartsWith(const char *pre, const char *str);
char* remove_all_chars(char* str, char c);
int strpos(char *haystack, char *needle);
int map(int x, int in_min, int in_max, int out_min, int out_max);
char** str_split(char* a_str, const char a_delim);
#endif