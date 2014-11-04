package
{
	public class Highlight
	{
		public function Highlight(source:SourceCode, from:int, to:int)
		{
			Source = source;
			From = from;
			To = to;
		}
		
		public var From:int;
		public var To:int;
		public var Source:SourceCode;

	}
}