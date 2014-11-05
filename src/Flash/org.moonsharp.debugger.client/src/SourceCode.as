package
{
	public class SourceCode
	{
		private var m_Id : int;
		private var m_Name : String;
		private var m_Lines : Number;
		private var m_Text : String;
		private var m_CumulativeLength : Vector.<int> = new Vector.<int>();
		
		public var Breakpoints : Vector.<Highlight> = new Vector.<Highlight>();
		
		public function SourceCode(xml: XML)
		{
			m_Id = xml.@id;
			m_Name = xml.@name.toString();
			
			var lines : XMLList = xml.elements();
			
			m_Text = "";
			
			for each (var line : XML in lines)
			{
				m_CumulativeLength.push(m_Text.length);
				m_Text += line.toString() + "\n";			
				m_Lines += 1;
			}
		}
		
		public function getId() : int
		{
			return m_Id;	
		}
		
		public function getName() : String
		{
			return m_Name;	
		}
		
		public function getText() : String
		{
			return m_Text;	
		}
		
		public function flattenLocation(line: int, col: int) : int
		{
			return m_CumulativeLength[line] + col;
		}
		
		
		public function inflateLocationLine(pos : int) : int 
		{
			for(var line:int = 0; line < m_CumulativeLength.length; line++)
			{
				if (pos < m_CumulativeLength[line])
					return line - 1;
			}
			
			return m_CumulativeLength.length - 1;
		}
		
		public function inflateLocationColumn(pos : int, line : int) : int
		{
			if (line <= 0) return pos;
			return pos - m_CumulativeLength[line];
		}
		
	}
}














