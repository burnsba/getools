/**
 * Copyright 2022 Ben Burns
*/
/**
 * This file is part of Gaudio.
 * 
 * Gaudio is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Gaudio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Gaudio. If not, see <https://www.gnu.org/licenses/>. 
*/
#ifndef _GAUDIO_PARSE_H_
#define _GAUDIO_PARSE_H_

#include "machine_config.h"

int is_whitespace(char c);
int is_newline(char c);
int is_alpha(char c);
int is_alphanumeric(char c);
int is_numeric(char c);
int is_numeric_int(char c);
int is_comment(char c);

int is_windows_newline(int c, int previous_c);

#endif