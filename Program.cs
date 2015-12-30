using System;
using System.IO;

namespace FitFileTranscoder
{
    class Program
    {
        /// <summary>
        /// Receives message events from a decoder and writes them to an encoder
        /// </summary>
        public class Transcoder : System.IDisposable
        {
            private readonly Dynastream.Fit.Sport   resultantSport;
            private readonly Dynastream.Fit.Decode  decoder;
            private readonly Dynastream.Fit.Encode  encoder;

            internal Transcoder(Dynastream.Fit.Decode decoder, Dynastream.Fit.Encode encoder, Dynastream.Fit.Sport resultantSport)
            {
                this.decoder = decoder;
                this.decoder.MesgEvent += new Dynastream.Fit.MesgEventHandler(this.HandleMessageEvent);
                this.decoder.MesgDefinitionEvent += new Dynastream.Fit.MesgDefinitionEventHandler(this.HandleMessageDefinitionEvent);
                this.encoder = encoder;
                this.resultantSport = resultantSport;
            }

            public void Dispose()
            {
                this.decoder.MesgEvent -= new Dynastream.Fit.MesgEventHandler(this.HandleMessageEvent);
                this.decoder.MesgDefinitionEvent -= new Dynastream.Fit.MesgDefinitionEventHandler(this.HandleMessageDefinitionEvent);
            }

            private void HandleMessageEvent(object sender, Dynastream.Fit.MesgEventArgs e)
            {
                Console.WriteLine("Handling message {0} {1}", e.mesg.Num, e.mesg.Name);
                if (Dynastream.Fit.MesgNum.Session == e.mesg.Num)
                {
                    var sessionMessage = new Dynastream.Fit.SessionMesg(e.mesg);
                    Console.WriteLine("Found session:\n{0}\nSetting sport to '{1}'..", sessionMessage, this.resultantSport);
                    sessionMessage.SetSport(resultantSport);
                    this.encoder.OnMesg(sessionMessage);
                }
                else
                {
                    // Pass through all other messages
                    this.encoder.OnMesg(e.mesg);
                }
            }

            private void HandleMessageDefinitionEvent(object sender, Dynastream.Fit.MesgDefinitionEventArgs e)
            {
                this.encoder.OnMesgDefinition(e.mesgDef);
            }
        }

        public static void TranscodeFile(string filePath, string outputPath, Dynastream.Fit.Sport resultantSport)
        {
            Console.WriteLine("Transcoding file {0} to {1}..", filePath, outputPath);
            using (var fitFileStream = new FileStream(filePath, FileMode.Open))
            using (var outputFitFileStream = new FileStream(outputPath, FileMode.Create))
            {
                var decoder = new Dynastream.Fit.Decode();
                var encoder = new Dynastream.Fit.Encode();
                using (var transcoder = new Transcoder(decoder, encoder, resultantSport))
                {
                    encoder.Open(outputFitFileStream);
                
                    bool status = decoder.IsFIT(fitFileStream) & decoder.CheckIntegrity(fitFileStream);

                    // Process the file
                    if (!status)
                    {
                        throw new ApplicationException(string.Format("Integrity Check Failed {0}", filePath));
                    }
                    else
                    {
                        decoder.Read(fitFileStream);
                    }

                    encoder.Close();
                }
            }
            Console.WriteLine("Transcoded file {0} to {1}..", filePath, outputPath);
        }

        struct ParsedArguments
        {
            public readonly Dynastream.Fit.Sport resultantSport;
            public readonly string inputFilePath;
            
            private static void PrintUsage()
            {
                // The fit file is passed in last so that it can be the target of a Windows SendTo
                Console.WriteLine("Argument 1: the name of the resultant activity type enum, e.g. Running, Cycling, Walking, Hiking");
                Console.WriteLine("Argument 2: a path to a fit file");
            }
    
            public ParsedArguments(string[] args)
            {
                try
                {
                    if (args.Length < 2)
                    {
                        PrintUsage();
                        throw new ArgumentException(string.Format("Not enough arguments - {0}!", args.Length));
                    }
                    else
                    {
                        resultantSport = (Dynastream.Fit.Sport)Enum.Parse(typeof(Dynastream.Fit.Sport), args[0]);
                        inputFilePath = args[1];
                    }
                }
                catch
                {
                    PrintUsage();
                    throw;
                }
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                var parsedArguments = new ParsedArguments(args);
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                string transcodedFilePath = Path.ChangeExtension(parsedArguments.inputFilePath, ".Transcoded.fit");
                string finalOriginalPath = Path.ChangeExtension(parsedArguments.inputFilePath, ".Original.fit");

                TranscodeFile(parsedArguments.inputFilePath, transcodedFilePath, parsedArguments.resultantSport);

                stopwatch.Stop();
                Console.WriteLine("");
                Console.WriteLine("Time elapsed: {0:0.#}s", stopwatch.Elapsed.TotalSeconds);

                Console.WriteLine("Transcode successful. Moving files..");

                File.Move(parsedArguments.inputFilePath, finalOriginalPath);
                Console.WriteLine("Moved original to {0}.", finalOriginalPath);

                File.Move(transcodedFilePath, parsedArguments.inputFilePath);
                Console.WriteLine("Moved transcoded to {0}.", parsedArguments.inputFilePath);

                Console.WriteLine("Done.");
            }
            catch (Exception caught)
            {
                Console.WriteLine("Execution failed with exception:\n{0}", caught.ToString());
            }
                    
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
    }
}