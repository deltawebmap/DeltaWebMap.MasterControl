using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.MasterControl.CLI
{
    static class CLITools
    {
        public static void PrintText(string text, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.WriteLine(text);
        }

        public static string PromptFormTextInput(string title, string help, string defaultValue = null)
        {
            PrintText(title, ConsoleColor.Blue);
            PrintText(help);
            return PromptString(defaultValue);
        }

        public static int PromptFormIntInput(string title, string help, string defaultValue = null)
        {
            PrintText(title, ConsoleColor.Blue);
            PrintText(help);
            return PromptInt(defaultValue);
        }

        public static string PromptFormSelect(string title, string help, params string[] values)
        {
            PrintText(title, ConsoleColor.Blue);
            PrintText(help);
            return PromptSelect(values);
        }

        public static string PromptSelect(params string[] options)
        {
            int top = Console.CursorTop;
            int left = Console.CursorLeft;
            int index = 0;
            while(true)
            {
                //Print
                int maxLen = Console.WindowWidth - left - 11;
                int len = Math.Min(maxLen, options[index].Length);
                Console.SetCursorPosition(left, top);
                Console.ForegroundColor = (index > 0) ? ConsoleColor.White : ConsoleColor.DarkGray;
                Console.Write("[<-] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(options[index].Substring(0, len));
                Console.ForegroundColor = (index < options.Length-1) ? ConsoleColor.White : ConsoleColor.DarkGray;
                Console.Write(" [->]");

                //Pad
                while(Console.CursorLeft < Console.WindowWidth - 1)
                    Console.Write(' ');

                //Read
                Console.SetCursorPosition(left, top);
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.LeftArrow && index > 0)
                    index--;
                if (k.Key == ConsoleKey.RightArrow && index < options.Length-1)
                    index++;
                if (k.Key == ConsoleKey.Enter)
                    break;
            }

            //Write new line
            Console.Write("\n");
            return options[index];
        }

        public static int PromptInt(string defaultValue)
        {
            int value;
            while (!int.TryParse(PromptString(defaultValue), out value)) ;
            return value;
        }

        public static string PromptString(string defaultValue = null)
        {
            //Write cursor
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("> ");

            //Write default, if any
            if (defaultValue != null)
            {
                int maxWritten = Console.WindowWidth - Console.CursorLeft - 1;
                int left = Console.CursorLeft;
                int top = Console.CursorTop;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(defaultValue.Substring(0, Math.Min(maxWritten, defaultValue.Length)));
                Console.SetCursorPosition(left, top);
            }

            //Prompt
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;
            Console.ForegroundColor = ConsoleColor.White;
            string response = "";
            bool defaultCleared = defaultValue == null;
            while (true)
            {
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Enter)
                    break;
                else if (k.Key == ConsoleKey.Backspace)
                {
                    if(response.Length > 0)
                    {
                        Console.Write(' ');
                        Console.CursorLeft--;
                        response = response.Substring(0, response.Length - 1);
                    }
                }
                else
                {
                    response += k.KeyChar;
                    if (!defaultCleared)
                    {
                        //Clear the default
                        int left = Console.CursorLeft;
                        for (int i = 0; i < Console.WindowWidth - Console.CursorLeft - 1; i++)
                            Console.Write(' ');
                        Console.CursorLeft = left;
                        defaultCleared = true;
                    }
                }
            }

            //Handle defaults
            if (response == "" && defaultValue != null)
            {
                //Use the default value. Write the 
                response = defaultValue;
                Console.SetCursorPosition(startLeft, startTop);
                Console.Write(defaultValue);
            }

            //Write new line
            Console.Write("\n");
            Console.Write("\n");
            return response;
        }
    }
}
