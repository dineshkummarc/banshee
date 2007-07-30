/*
 *  Copyright (c) 2007 Scott Peterson <lunchtimemama@gmail.com> 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */ 

using System;
using System.Collections.Generic;
using System.Text;

namespace Banshee.PlayerMigration
{
    internal struct ItunesSmartPlaylist
    {
        public string Query, Ignore, OrderBy, Name, Output;
        public uint LimitNumber;
        public byte LimitMethod;
    }

    internal class SmartPlaylistParser
    {
        private struct Kind
        {
            public string Name, Extension;
            public Kind(string name, string extension)
            {
                Name = name;
                Extension = extension;
            }
        }
        
        //The methods by which the number of songs in a playlist are limited
        private enum LimitMethods
        {
            Minutes = 0x01,
            MB = 0x02,
            Items = 0x03,
            Hours = 0x04,
            GB = 0x05,
        }
        
        // The methods by which songs are selected for inclusion in a limited playlist
        private enum SelectionMethods
        {
            Random = 0x02,
            Title = 0x05,
            AlbumTitle = 0x06,
            Artist = 0x07,
            Genre = 0x09,
            HighestRating = 0x1c,
            LowestRating = 0x01,
            RecentlyPlayed = 0x1a,
            OftenPlayed = 0x19,
            RecentlyAdded = 0x15
        }
        
        // The matching criteria which take string data
        private enum StringFields
        {
            AlbumTitle = 0x03,
            AlbumArtist = 0x47,
            Artist = 0x04,
            Category = 0x37,
            Comments = 0x0e,
            Composer = 0x12,
            Description = 0x36,
            Genre = 0x08,
            Grouping = 0x27,
            Kind = 0x09,
            Title = 0x02,
            Show = 0x3e
        }
        
        // The matching criteria which take integer data
        private enum IntFields
        {
            BPM = 0x23,
            BitRate = 0x05,
            Compilation = 0x1f,
            DiskNumber = 0x18,
            NumberOfPlays = 0x16,
            Rating = 0x19,
            Playlist = 0x28,    // FIXME Move this?
            Podcast = 0x39,
            SampleRate = 0x06,
            Season = 0x3f,
            Size = 0x0c,
            SkipCount = 0x44,
            Duration = 0x0d,
            TrackNumber = 0x0b,
            VideoKind = 0x3c,
            Year = 0x07
        }
        
        // The matching criteria which take date data
        private enum DateFields
        {
            DateAdded = 0x10,
            DateModified = 0x0a,
            LastPlayed = 0x17,
            LastSkipped = 0x45
        }
        
        // The string matching criteria which we do no handle
        private enum IgnoreStringFields
        {
            AlbumArtist = 0x47,
            Category = 0x37,
            Comments = 0x0e,
            Composer = 0x12,
            Description = 0x36,
            Grouping = 0x27,
            Show = 0x3e
        }
        
        // The integer matching criteria which we do no handle
        private enum IgnoreIntFields
        {
            BPM = 0x23,
            BitRate = 0x05,
            Compilation = 0x1f,
            DiskNumber = 0x18,
            Playlist = 0x28,
            Podcast = 0x39,
            SampleRate = 0x06,
            Season = 0x3f,
            Size = 0x0c,
            SkipCount = 0x44,
            TrackNumber = 0x0b,
            VideoKind = 0x3c
        }
        
        // The date matching criteria which we do no handle
        private enum IgnoreDateFields
        {
            DateModified = 0x0a,
            LastSkipped = 0x45
        }
        
        // The signs which apply to different kinds of logic (is vs. is not, contains vs. doesn't contain, etc.)
        private enum LogicSign
        {
            IntPositive = 0x00,
            StringPositive = 0x01,
            IntNegative = 0x02,
            StringNegative = 0x03
        }
        
        // The logical rules
        private enum LogicRule
        {
            Other = 0x00,
            Is = 0x01,
            Contains = 0x02,
            Starts = 0x04,
            Ends = 0x08,
            Greater = 0x10,
            Less = 0x40
        }
        
        private static Kind[] Kinds = {
            new Kind("Protected AAC audio file", ".m4p"),
            new Kind("MPEG audio file", ".mp3"),
            new Kind("AIFF audio file", ".aiff"),
            new Kind("WAV audio file", ".wav"),
            new Kind("QuickTime movie file", ".mov"),
            new Kind("MPEG-4 video file", ".mp4"),
            new Kind("AAC audio file", ".m4a")
        };
        
        private delegate bool KindEvalDel(Kind kind, string query);
        
        // INFO OFFSETS
        //
        // Offsets for bytes which...
        const int MATCHBOOLOFFSET = 1;           // determin whether logical matching is to be performed - Absolute offset
        const int LIMITBOOLOFFSET = 2;           // determin whether results are limited - Absolute offset
        const int LIMITMETHODOFFSET = 3;         // determin by what criteria the results are limited - Absolute offset
        const int SELECTIONMETHODOFFSET = 7;     // determin by what criteria limited playlists are populated - Absolute offset
        const int LIMITINTOFFSET = 11;           // determin the limited - Absolute offset
        const int SELECTIONMETHODSIGNOFFSET = 13;// determin whether certain selection methods are "most" or "least" - Absolute offset 

        // CRITERIA OFFSETS
        //
        // Offsets for bytes which...
        const int LOGICTYPEOFFSET = 15;   // determin whether all or any criteria must match - Absolute offset
        const int FIELDOFFSET = 139;      // determin what is being matched (Artist, Album, &c) - Absolute offset
        const int LOGICSIGNOFFSET = 1;    // determin whether the matching rule is positive or negative (e.g., is vs. is not) - Relative offset from FIELDOFFSET
        const int LOGICRULEOFFSET = 4;    // determin the kind of logic used (is, contains, begins, &c) - Relative offset from FIELDOFFSET
        const int STRINGOFFSET = 54;      // begin string data - Relative offset from FIELDOFFSET
        const int INTAOFFSET = 60;        // begin the first int - Relative offset from FIELDOFFSET
        const int INTBOFFSET = 24;        // begin the second int - Relative offset from INTAOFFSET
        const int TIMEMULTIPLEOFFSET = 76;// begin the int with the multiple of time - Relative offset from FIELDOFFSET
        const int TIMEVALUEOFFSET = 68;   // begin the inverse int with the value of time - Relative offset from FIELDOFFSET

        const int INTLENGTH = 64;         // The length on a int criteria starting at the first int
        static DateTime STARTOFTIME = new DateTime(1904, 1, 1); // Dates are recorded as seconds since Jan 1, 1904

        bool or, again;
        string conjunction_output, conjunction_query, output, query, ignore;
        int offset, logic_sign_offset, logic_rules_offset, string_offset, int_a_offset, int_b_offset,
            time_multiple_offset, time_value_offset;
        byte[] info, criteria;

        KindEvalDel KindEval;

        public ItunesSmartPlaylist Parse(byte[] i, byte[] c)
        {
            info = i;
            criteria = c;
            ItunesSmartPlaylist result = new ItunesSmartPlaylist();
            offset = FIELDOFFSET;
            output = query = ignore = string.Empty;

            if(info[MATCHBOOLOFFSET] == 1) {
                or = (criteria[LOGICTYPEOFFSET] == 1) ? true : false;
                if(or) {
                    conjunction_query = " OR ";
                    conjunction_output = " or\n";
                } else {
                    conjunction_query = " AND ";
                    conjunction_output = " and\n";
                }
                do {
                    again = false;
                    logic_sign_offset = offset + LOGICSIGNOFFSET;
                    logic_rules_offset = offset + LOGICRULEOFFSET;
                    string_offset = offset + STRINGOFFSET;
                    int_a_offset = offset + INTAOFFSET;
                    int_b_offset = int_a_offset + INTBOFFSET;
                    time_multiple_offset = offset + TIMEMULTIPLEOFFSET;
                    time_value_offset = offset + TIMEVALUEOFFSET;
                    
                    if(Enum.IsDefined(typeof(StringFields), (int)criteria[offset])) {
                        ProcessStringField();
                    } else if(Enum.IsDefined(typeof(IntFields), (int)criteria[offset])) {
                        ProcessIntField();
                    } else if(Enum.IsDefined(typeof(DateFields), (int)criteria[offset])) {
                        ProcessDateField();
                    } else {
                        ignore += "Not processed";
                    }
                }
                while(again);
            }
            result.Output = output;
            result.Query = query;
            result.Ignore = ignore;
            if(info[LIMITBOOLOFFSET] == 1) {
                uint limit = BytesToUInt(info, LIMITINTOFFSET);
                result.LimitNumber = (info[LIMITMETHODOFFSET] == (byte)LimitMethods.GB) ? limit * 1024 : limit;
                if(output.Length > 0) {
                      output += "\n";
                }
                output += "Limited to " + limit.ToString() + " " +
                    Enum.GetName(typeof(LimitMethods), (int)info[LIMITMETHODOFFSET]) + " selected by ";
                switch(info[LIMITMETHODOFFSET]) {
                case (byte)LimitMethods.Items:
                    result.LimitMethod = 0;
                    break;
                case (byte)LimitMethods.Minutes:
                    result.LimitMethod = 1;
                    break;
                case (byte)LimitMethods.Hours:
                    result.LimitMethod = 2;
                    break;
                case (byte)LimitMethods.MB:
                    result.LimitMethod = 3;
                    break;
                case (byte)LimitMethods.GB:
                    goto case (byte)LimitMethods.MB;
                }
                switch(info[SELECTIONMETHODOFFSET]) {
                case (byte)SelectionMethods.Random:
                    output += "random";
                    result.OrderBy = "RANDOM()";
                    break;
                case (byte)SelectionMethods.HighestRating:
                    output += "highest rated";
                    result.OrderBy = "Rating DESC";
                    break;
                case (byte)SelectionMethods.LowestRating:
                    output += "lowest rated";
                    result.OrderBy = "Rating ASC";
                    break;
                case (byte)SelectionMethods.RecentlyPlayed:
                    output += (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "most recently played" : "least recently played";
                    result.OrderBy = (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "LastPlayedStamp DESC" : "LastPlayedStamp ASC";
                    break;
                case (byte)SelectionMethods.OftenPlayed:
                    output += (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "most often played" : "least often played";
                    result.OrderBy = (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "NumberOfPlays DESC" : "NumberOfPlays ASC";
                    break;
                case (byte)SelectionMethods.RecentlyAdded:
                    output += (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "most recently added" : "least recently added";
                    result.OrderBy = (info[SELECTIONMETHODSIGNOFFSET] == 0)
                        ? "DateAddedStamp DESC" : "DateAddedStamp ASC";
                    break;
                default:
                    result.OrderBy = Enum.GetName(typeof(SelectionMethods), (int)info[SELECTIONMETHODOFFSET]);
                    break;
                }
            }
            if(ignore.Length > 0) {
                output += "\n\nIGNORING:\n" + ignore;
            }
            
            if(query.Length > 0) {
                output += "\n\nQUERY:\n" + query;
            }
            return result;
        }

        private void ProcessStringField()
        {
            bool end = false;
               string working_output = Enum.GetName(typeof(StringFields), criteria[offset]);
               string working_query = "(lower(" + Enum.GetName(typeof(StringFields), criteria[offset]) + ")";
               switch(criteria[logic_rules_offset]) {
               case (byte)LogicRule.Contains:
                   if((criteria[logic_sign_offset] == (byte)LogicSign.StringPositive)) {
                         working_output += " contains ";
                         working_query += " LIKE '%";
                     } else {
                       working_output += " does not contain ";
                       working_query += " NOT LIKE '%";
                   }
                   if(criteria[offset] == (byte)StringFields.Kind) {
                       KindEval = delegate(Kind kind, string query) {
                           return (kind.Name.IndexOf(query) != -1);
                       };
                   }
                   end = true;
                   break;
               case (byte)LogicRule.Is:
                   if((criteria[logic_sign_offset] == (byte)LogicSign.StringPositive)) {
                       working_output += " is ";
                       working_query += " = '";
                   } else {
                       working_output += " is not ";
                       working_query += " != '";
                   }
                   if(criteria[offset] == (byte)StringFields.Kind) {
                       KindEval = delegate(Kind kind, string query) {
                           return (kind.Name == query);
                       };
                   }
                   break;
               case (byte)LogicRule.Starts:
                   working_output += " starts with ";
                   working_query += " LIKE '";
                   if(criteria[offset] == (byte)StringFields.Kind) {
                       KindEval = delegate (Kind kind, string query) {
                           return (kind.Name.IndexOf(query) == 0);
                       };
                   }
                   end = true;
                   break;
               case (byte)LogicRule.Ends:
                   working_output += " ends with ";
                   working_query += " LIKE '%";
                   if(criteria[offset] == (byte)StringFields.Kind) {
                       KindEval = delegate (Kind kind, string query) {
                           return (kind.Name.IndexOf(query) == (kind.Name.Length - query.Length));
                       };
                   }
                   break;
            }
            working_output += "\"";
            byte[] character = new byte[1];
            string content = String.Empty;
            bool onByte = true;
            for(int i = (string_offset); i < criteria.Length; i++) {
                // Off bytes are 0
                if(onByte) {
                    // If the byte is 0 and it's not the last byte,
                    // we have another condition
                    if(criteria[i] == 0 && i != (criteria.Length - 1)) {
                        again = true;
                        FinishStringField(content, working_output, working_query, end);
                        offset = i + 2;
                        return;
                    }
                    character[0] = criteria[i];
                    content += Encoding.UTF8.GetString(character);
                }
                onByte = !onByte;
            }
            FinishStringField(content, working_output, working_query, end);
        }

        private void FinishStringField(string content, string working_output, string working_query, bool end)
        {
               working_output += content;
               working_output += "\" ";
               bool failed = false;
               if(criteria[offset] == (byte)StringFields.Kind) {
                   working_query = string.Empty;
                foreach(Kind kind in Kinds) {
                    if(KindEval(kind, content)) {
                        if(working_query.Length > 0) {
                            if((query.Length == 0 && !again) || or) {
                                working_query += " OR ";
                            } else {
                                failed = true;
                                break;
                            }
                        }
                        working_query += "(lower(Uri)";
                        working_query += ((criteria[logic_sign_offset] == (byte)LogicSign.StringPositive))
                            ? " LIKE '%" + kind.Extension + "')" : " NOT LIKE '%" + kind.Extension + "%')";
                    }
                }
               } else {
                   working_query += content.ToLower();
                   working_query += (end) ? "%')" : "')";
               }
            if(Enum.IsDefined(typeof(IgnoreStringFields),
                (int)criteria[offset]) || failed) {
                if(ignore.Length > 0) {
                    ignore += conjunction_output;
                }
                ignore += working_output;
            } else {
                if(output.Length > 0) {
                    output += conjunction_output;
                }
                if(query.Length > 0) {
                    query += conjunction_query;
                }
                output += working_output;
                query += working_query;
            }
        }

        private void ProcessIntField()
        {
               string working_output = Enum.GetName(typeof(IntFields), criteria[offset]);
               string working_query = "(" + Enum.GetName(typeof(IntFields), criteria[offset]);
               
               switch(criteria[logic_rules_offset]) {
               case (byte)LogicRule.Is:
                   if(criteria[logic_sign_offset] == (byte)LogicSign.IntPositive) {
                       working_output += " is ";
                       working_query += " = ";
                   } else {
                       working_output += " is not ";
                       working_query += " != ";
                   }
                   goto case 255;
               case (byte)LogicRule.Greater:
                   working_output += " is greater than ";
                   working_query += " > ";
                   goto case 255;
               case (byte)LogicRule.Less:
                   working_output += " is less than ";
                   working_query += " > ";
                   goto case 255;
               case 255:
                   ulong number = (criteria[offset] == (byte)IntFields.Rating)
                       ? (BytesToUInt(criteria, int_a_offset) / 20) : BytesToUInt(criteria, int_a_offset);
                   working_output += number.ToString();
                   working_query += number.ToString();
                   break;
               case (byte)LogicRule.Other:
                   if(criteria[logic_sign_offset + 2] == 1) {
                       working_output += " is in the range of ";
                       working_query += " BETWEEN ";
                       ulong num = (criteria[offset] == (byte)IntFields.Rating)
                           ? (BytesToUInt(criteria, int_a_offset) / 20) : BytesToUInt(criteria, int_a_offset);
                       working_output += num.ToString();
                       working_query += num.ToString();
                       working_output += " to ";
                       working_query += " AND ";
                       num = (criteria[offset] == (byte)IntFields.Rating)
                           ? ((BytesToUInt(criteria, int_b_offset) - 19) / 20) : BytesToUInt(criteria, int_b_offset);
                       working_output += num.ToString();
                       working_query += num.ToString();
                   }
                   break;
            }
               working_query += ")";
               if(Enum.IsDefined(typeof(IgnoreIntFields),
                   (int)criteria[offset])) {
                   if(ignore.Length > 0) {
                       ignore += conjunction_output;
                   }
                   ignore += working_output;
               } else {
                   if(output.Length > 0) {
                       output += conjunction_output;
                   }
                   if(query.Length > 0) {
                       query += conjunction_query;
                   }
                   output += working_output;
                   query += working_query;
               }
               offset = int_a_offset + INTLENGTH;
               if(criteria.Length > offset) {
                   again = true;
            }
        }

        private void ProcessDateField()
        {
            bool isIgnore = false;
               string working_output = Enum.GetName(typeof(DateFields), criteria[offset]);
               string working_query = "((strftime(\"%s\", current_timestamp) - DateAddedStamp + 3600)";
               switch(criteria[logic_rules_offset]) {
               case (byte)LogicRule.Greater:
                   working_output += " is after ";
                   working_query += " > ";
                   goto case 255;
               case (byte)LogicRule.Less:
                   working_output += " is before ";
                   working_query += " > ";
                   goto case 255;
               case 255:
                   isIgnore = true;
                   DateTime time = BytesToDateTime(criteria, int_a_offset);
                   working_output += time.ToString();
                   working_query += ((int)DateTime.Now.Subtract(time).TotalSeconds).ToString();
                   break;
               case (byte)LogicRule.Other:
                   if(criteria[logic_sign_offset + 2] == 1) {
                       isIgnore = true;
                       DateTime t2 = BytesToDateTime(criteria, int_a_offset);
                       DateTime t1 = BytesToDateTime(criteria, int_b_offset);
                       if(criteria[logic_sign_offset] == (byte)LogicSign.IntPositive) {
                           working_output += " is in the range of ";
                           working_query += " BETWEEN " +
                               ((int)DateTime.Now.Subtract(t1).TotalSeconds).ToString() +
                               " AND " +
                               ((int)DateTime.Now.Subtract(t2).TotalSeconds).ToString();
                       } else {
                           working_output += " is not in the range of ";
                       }
                       working_output += t1.ToString();
                       working_output += " to ";
                       working_output += t2.ToString();
                   } else if(criteria[logic_sign_offset + 2] == 2) {
                       if(criteria[logic_sign_offset] == (byte)LogicSign.IntPositive) {
                           working_output += " is in the last ";
                           working_query += " < ";
                       } else {
                           working_output += " is not in the last ";
                           working_query += " > ";
                       }
                       ulong t = InverseBytesToUInt(criteria, time_value_offset);
                       ulong multiple = BytesToUInt(criteria, time_multiple_offset);
                       working_query += (t * multiple).ToString();
                       working_output += t.ToString() + " ";
                       switch(multiple) {
                       case 86400:
                           working_output += "days";
                           break;
                       case 604800:
                           working_output += "weeks";
                           break;
                       case 2628000:
                           working_output += "months";
                            break;
                       }
                   }
                   break;
            }
            working_query += ")";
            if(isIgnore || Enum.IsDefined(typeof(IgnoreDateFields), (int)criteria[offset])) {
                if(ignore.Length > 0) {
                    ignore += conjunction_output;
                }
                ignore += working_output;
            } else {
                if(output.Length > 0) {
                    output += conjunction_output;
                }
                output += working_output;
                if(query.Length > 0) {
                    query += conjunction_query;
                }
                query += working_query;
            }          
            offset = int_a_offset + INTLENGTH;
            if(criteria.Length > offset) {
                again = true;
            }
        }

        /// <summary>
        /// Converts 4 bytes to a uint
        /// </summary>
        /// <param name="byteArray">A byte array</param>
        /// <param name="offset">Should be the byte of the uint with the 0th-power position</param>
        /// <returns></returns>
        private static uint BytesToUInt(byte[] byteArray, int offset)
        {
            uint output = 0;
            for (byte i = 0; i < 4; i++) {
                output += (uint)(byteArray[offset - i] * Math.Pow(2, (8 * i)));
            }
            return output;
        }

        private static uint InverseBytesToUInt(byte[] byteArray, int offset)
        {
            uint output = 0;
            for (byte i = 0; i <= 4; i++) {
                output += (uint)((255 - (uint)(byteArray[offset - i])) * Math.Pow(2, (8 * i)));
            }
            return ++output;
        }

        private static DateTime BytesToDateTime (byte[] byteArray, int offset)
        {
            return STARTOFTIME.AddSeconds(BytesToUInt(byteArray, offset));
        }
    }
}
