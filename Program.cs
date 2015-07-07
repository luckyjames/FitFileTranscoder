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
			private readonly Dynastream.Fit.Decode decoder;
			private readonly Dynastream.Fit.Encode encoder;

			internal Transcoder(Dynastream.Fit.Decode decoder, Dynastream.Fit.Encode encoder)
			{
				this.decoder = decoder;
				this.decoder.MesgEvent += new Dynastream.Fit.MesgEventHandler(this.HandleMessageEvent);
				this.decoder.MesgDefinitionEvent += new Dynastream.Fit.MesgDefinitionEventHandler(this.HandleMessageDefinitionEvent);
				this.encoder = encoder;
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
					Console.WriteLine("Found session:\n{0}\nSetting sport to running..", sessionMessage);
					sessionMessage.SetSport(Dynastream.Fit.Sport.Running);
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

		public static void TranscodeFile(string filePath, string outputPath)
		{
			Console.WriteLine("Transcoding file {0} to {1}..", filePath, outputPath);
			using (var fitFileStream = new FileStream(filePath, FileMode.Open))
			using (var outputFitFileStream = new FileStream(outputPath, FileMode.Create))
			{
				var decoder = new Dynastream.Fit.Decode();
				var encoder = new Dynastream.Fit.Encode();
				using (var transcoder = new Transcoder(decoder, encoder))
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

		public static void Main(string[] args)
		{
			try
			{
				if (args.Length < 1)
				{
					throw new ArgumentException("Expecting one argument that is a path to a fit file!");
				}
				else
				{
					var stopwatch = new System.Diagnostics.Stopwatch();
					stopwatch.Start();

					string inputFilePath = args[0];					
					string transcodedFilePath = Path.ChangeExtension(args[0], ".Transcoded.fit");
					TranscodeFile(inputFilePath, transcodedFilePath);

					stopwatch.Stop();
					Console.WriteLine("");
					Console.WriteLine("Time elapsed: {0:0.#}s", stopwatch.Elapsed.TotalSeconds);

					Console.WriteLine("Transcode successful. Moving files..");
					File.Move(inputFilePath, Path.ChangeExtension(inputFilePath, ".Original.fit"));
					File.Move(transcodedFilePath, inputFilePath);
					Console.WriteLine("Done.");
				}
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