using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrtMerge
{
    public class SrtBlock
    {
        public int Index;
        public TimeSpan Start;
        public TimeSpan End;
        public string Content;

        public SrtBlock( List<string> text )
        {
            var toks = text[1].Split( "-->", StringSplitOptions.TrimEntries );

            Index = int.Parse( text[0] );
            Start = TimeSpan.Parse( toks[0] );
            End = TimeSpan.Parse( toks[1] );
            Content = string.Join( Environment.NewLine, text.Skip( 2 ) );
        }

        public SrtBlock()
        {
        }

        public SrtBlock( int index, TimeSpan start, TimeSpan end, string content )
        {
            Index = index;
            Start = start;
            End = end;
            Content = content;
        }

        public string GetBlock()
        {
            string output = Index.ToString() + Environment.NewLine;
            output += $"{Start.ToString( "hh\\:mm\\:ss\\,fff" )} --> {End.ToString( "hh\\:mm\\:ss\\,fff" )}" + Environment.NewLine;
            output += Content + Environment.NewLine;

            return output;
        }
    }

    public class SrtFile
    {
        public List<SrtBlock> Blocks;

        public SrtFile()
        {
            Blocks = new();
        }

        public SrtFile( List<string> data )
        {
            Blocks = new();

            for ( int i = 0; i < data.Count; i++ )
            {
                var t = data[i];

                if ( !string.IsNullOrWhiteSpace( t ) && int.TryParse( t, out var _ ) )
                {
                    for ( int j = i + 1; j < data.Count; j++ )
                    {
                        var t2 = data[j];

                        if ( string.IsNullOrWhiteSpace( t2 ) )
                        {
                            Blocks.Add( new SrtBlock( data.Skip( i ).Take( j - i ).ToList() ) );

                            i = j;

                            break;
                        }
                    }
                }
            }
        }

        public string GetFile()
        {
            StringWriter output = new();

            foreach( var b in Blocks)
            {
                output.Write( b.GetBlock() );
                output.Write( Environment.NewLine );
            }

            return output.ToString();
        }
    }

}
