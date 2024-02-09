/*
   MIT License
   
   Copyright (c) 2024 Alastair Lundy
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
 */

namespace CommandLineArgsParser
{
    public class UnixLikeCliParser
    {
        public static CommandLineInput[] ParseCliInputs(string[] args)
        {
            Dictionary<string, CommandLineInputType> categorizedOptions = GetInputTypes(args);
            
            List<CommandLineInput> commandLineInputs = new List<CommandLineInput>();

            CommandLineInput commandLineInput = new CommandLineInput();

            bool previousInputWasOptionName = false;
            
            for (int index = 0; index < categorizedOptions.Count; index++)
            {
                var inputType = categorizedOptions[args[index]];

                if (inputType == CommandLineInputType.OptionName)
                {
                    if (index > 0)
                    {
                        commandLineInputs.Add(commandLineInput);
                    }
                    
                    commandLineInput = new CommandLineInput();
                    
                    commandLineInput.OptionName = args[index];
                    previousInputWasOptionName = true;
                }
                else
                {
                    if (previousInputWasOptionName && inputType == CommandLineInputType.OptionParameter)
                    {
                        commandLineInput.OptionArguments.Add(args[index]);
                    }
                    else
                    {
                        commandLineInputs.Add(commandLineInput);
                        
                        commandLineInput = new CommandLineInput();
                        commandLineInput.OptionName = args[index];
                    }
                }
                
            }

            return commandLineInputs.ToArray();
        }

        /// <summary>
        /// Get the type of CLI argument 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Dictionary<string, CommandLineInputType> GetInputTypes(string[] args)
        {
            Dictionary<string, CommandLineInputType> commandLineInputTypes = new Dictionary<string, CommandLineInputType>();

            for (int index = 0; index < args.Length; index++)
            {
                var argReverseIndex = (args.Length - (index + 1) >= 0) ? args.Length - (index + 1) : 0;
                var arg = args[argReverseIndex];
                
#if NETSTANDARD2_0
                if (arg.StartsWith("-") || arg.StartsWith("--"))
#elif NET6_0_OR_GREATER
                if (arg.StartsWith('-') || arg.StartsWith("--"))
#endif
                {
                    commandLineInputTypes.Add(arg, CommandLineInputType.OptionName);
                }
                else
                {
                    commandLineInputTypes.Add(arg, CommandLineInputType.OptionParameter);
                }
            }
            
            /*
             * Go over the arguments array again to detect Flags that may have been missed previously.
             */
            for (int index = 0; index < args.Length; index++)
            {
                var arg = args[index];

                var nextArgIndex = ((index + 1) < args.Length) ? index + 1 : index;
                
#if NETSTANDARD2_0
                if (arg.StartsWith("-") || arg.StartsWith("--"))
#elif NET6_0_OR_GREATER
                if (arg.StartsWith('-') || arg.StartsWith("--"))
#endif
                {
                    //If the current input and next input appear to both be options then the current input is actually a flag.
                    if (args[nextArgIndex].StartsWith("--") && index < args.Length)
                    {
                        commandLineInputTypes[args[index]] = CommandLineInputType.Flag;
                    }

                }
            }

            return commandLineInputTypes;
        }

}
}