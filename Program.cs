namespace SrtMerge
{
    internal class Program
    {
        static SrtFile CompactData( SrtFile input )
        {
            var InputData = input;

            string CurrentText = "";
            SrtFile CondensedData = new();
            TimeSpan LastTime = new TimeSpan( 0, 0, 0, 0, 0 );
            int LastIndex = 1;

            for ( int iblock = 0; iblock < InputData.Blocks.Count; iblock++ )
            {
                var b = InputData.Blocks[iblock];

                if ( CurrentText.Length + b.Content.Length > 500 )
                {
                    CondensedData.Blocks.Add( new SrtBlock( LastIndex, LastTime, LastTime + new TimeSpan( 0, 1, 30 ), CurrentText ) );

                    LastTime = LastTime + new TimeSpan( 0, 1, 30 );
                    LastIndex++;
                    CurrentText = b.Content;
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( CurrentText ) )
                    {
                        CurrentText += " ";
                    }

                    CurrentText += b.Content;
                }
            }

            if ( !string.IsNullOrWhiteSpace( CurrentText ) )
            {
                CondensedData.Blocks.Add( new SrtBlock( LastIndex, LastTime, LastTime + new TimeSpan( 0, 1, 30 ), CurrentText ) );
            }

            return CondensedData;
        }

        static SrtBlock MergeBlocks( List<SrtBlock> blocks, int idx, TimeSpan start, int duration )
        {
            SrtBlock o = new();

            o.Start = start;
            o.End = o.Start + new TimeSpan( 0, 0, duration );

            o.Index = idx;
            o.Content = string.Join( " ", blocks.Select( x => x.Content ) );

            return o;
        }

        static SrtFile CompactData2( SrtFile input, int lineLen, int duration, bool ignoreEnding )
        {
            SrtFile CondensedData = new();
            List<(int, bool)> DataProcess = new();

            foreach ( var b in input.Blocks )
            {
                var goodEnding = b.Content.Trim().EndsWith( "." ) || b.Content.Trim().EndsWith( "?" ) || b.Content.Trim().EndsWith( ";" );

                if ( ignoreEnding )
                {
                    goodEnding = true;
                }

                DataProcess.Add( (b.Content.Length, goodEnding) );
            }

            bool done = false;
            int startIdx = 0;
            int blockIdx = 1;
            TimeSpan blockStart = new TimeSpan( 0, 0, 0 );

            while ( !done )
            {
                int acc = 0;
                int i;

                for ( i = startIdx; i < DataProcess.Count; i++ )
                {
                    acc += DataProcess[i].Item1;
                    acc += 1;

                    if ( acc >= lineLen )
                    {
                        break;
                    }
                }

                if ( i >= DataProcess.Count )
                {
                    CondensedData.Blocks.Add( MergeBlocks( input.Blocks.Skip( startIdx ).ToList(), blockIdx, blockStart, duration ) );
                    done = true;
                }
                else
                {
                    var lastValid = i - 1;

                    for ( ; lastValid > startIdx; lastValid-- )
                    {
                        if ( DataProcess[lastValid].Item2 )
                        {
                            break;
                        }
                    }

                    // Merge
                    CondensedData.Blocks.Add( MergeBlocks( input.Blocks.Skip( startIdx ).Take( 1 + lastValid - startIdx ).ToList(), blockIdx, blockStart, duration ) );
                    blockIdx++;
                    blockStart += new TimeSpan( 0, 0, duration );

                    startIdx = lastValid + 1;
                }

            }

            return CondensedData;
        }

        static void ShowHelp()
        {
            Console.WriteLine( "Usage:" );
            Console.WriteLine( "SrtMerge [options] [InputFile] [OutputFile]" );
            Console.WriteLine( "" );
            Console.WriteLine( "Options:" );
            Console.WriteLine( "-l [lenght]      Change merge block length (Default 500)" );
            Console.WriteLine( "-d [duration s]  Change the blocks' duration in the output (Default 60)" );
            Console.WriteLine( "-a               Skip checks for sentence end" );
            Console.WriteLine( "-h               Show this help" );

            Console.WriteLine( "" );
            Console.WriteLine( "Example:" );
            Console.WriteLine( "> SrtMerge -d 75 input.srt converted.srt" );
            Console.WriteLine( "" );
        }

        static void Main( string[] args )
        {
            Console.WriteLine( "Srt Merger v1.02 - redy81" );

            if ( args.Length < 2 )
            {
                Console.WriteLine( "Not enoug parameters!" );
                Console.WriteLine();
                ShowHelp();
                return;
            }

            if ( args.Any( x => x == "-h" ) )
            {
                ShowHelp();
                return;
            }

            string InputFile = "";
            string OutputFile = "";
            bool IgnoreEndings = false;
            int LineLength = 500;
            int Duration = 60;

            for ( int i = 0; i < args.Length; i++ )
            {
                switch ( args[i].ToLower() )
                {
                    case "-a":
                        IgnoreEndings = true;
                        break;

                    case "-l":
                        try
                        {
                            LineLength = int.Parse( args[i + 1] );
                            i++;
                        }
                        catch
                        {
                            Console.WriteLine( "Invalid -l value" );
                            return;
                        }
                        break;

                    case "-d":
                        try
                        {
                            Duration = int.Parse( args[i + 1] );
                            i++;
                        }
                        catch
                        {
                            Console.WriteLine( "Invalid -d value" );
                            return;
                        }
                        break;

                    default:
                        {
                            if ( string.IsNullOrWhiteSpace( InputFile ) )
                            {
                                InputFile = args[i];
                            }
                            else if ( string.IsNullOrWhiteSpace( OutputFile ) )
                            {
                                OutputFile = args[i];
                            }
                        }
                        break;
                }
            }


            var InputSrtFile = File.ReadAllLines( InputFile );

            var InputData = new SrtFile( InputSrtFile.ToList() );

            var CondensedData = CompactData2( InputData, LineLength, Duration, IgnoreEndings );

            Console.WriteLine();

            File.WriteAllText( OutputFile, CondensedData.GetFile() );

            Console.WriteLine( $"Done! Blocks: {InputData.Blocks.Count} => {CondensedData.Blocks.Count}" );
        }
    }
}
