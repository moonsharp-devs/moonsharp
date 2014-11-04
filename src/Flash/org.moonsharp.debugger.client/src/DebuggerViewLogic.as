package
{
	import flash.events.*;
	import flash.external.ExternalInterface;
	import flash.net.XMLSocket;
	import flash.utils.Dictionary;
	
	import mx.collections.ArrayList;
	import mx.controls.Alert;
	import mx.controls.List;

	public class DebuggerViewLogic
	{
		private var m_View : Main;
		private var m_Socket : XMLSocket;
		
		private var m_Sources : Dictionary = new Dictionary();
		private var m_SourceList : ArrayList = new ArrayList();
		
		private var m_InstructionPtrHighlight : Highlight = null;
		
		
		public function DebuggerViewLogic(view : Main)
		{
			m_View = view;
			
			m_Socket = new XMLSocket("127.0.0.1", 20001);
			
			m_Socket.addEventListener(Event.CLOSE, closeHandler);
			m_Socket.addEventListener(Event.CONNECT, connectHandler);
			m_Socket.addEventListener(DataEvent.DATA, dataHandler);
			m_Socket.addEventListener(IOErrorEvent.IO_ERROR, ioErrorHandler);
			m_Socket.addEventListener(ProgressEvent.PROGRESS, progressHandler);
			m_Socket.addEventListener(SecurityErrorEvent.SECURITY_ERROR, securityErrorHandler);
		}
		
		private function closeHandler(event:Event):void {
			onFatalError("Connection closed.");
		}
		
		private function connectHandler(event:Event):void {
			trace("connectHandler: " + event);
			logMessage("Connection with host established.");
		}
		
		private function dataHandler(event:DataEvent):void {
		
			var xml:XML = new XML(event.data);
			
			var cmd:String = xml.name();
			var list:ArrayList;
			
			if (cmd == "welcome")
			{
				m_View.welcome(xml.@app, xml.@moonsharpver);	
				refresh();
			}
			else if (cmd == "source-code") 
			{
				var s : SourceCode = new SourceCode(xml);
				m_Sources[s.getId()] = s;
				m_SourceList.addItem(s);
				m_View.refreshSourceCode(s, m_SourceList);
			}
			else if (cmd == "source-loc")
			{
				m_InstructionPtrHighlight = parseHighlight(xml);
				m_View.refreshInstructionPtrHighlight(true);
			}			
			else if (cmd == "execution-completed")
			{
				m_InstructionPtrHighlight = null;
				m_View.refreshInstructionPtrHighlight(true);
				logMessage("Execution completed.");
			}
			else if (cmd == "callstack")
			{
				list = parseWatchData(xml);
				m_View.refreshCallStack(list);	
			}
			else if (cmd == "watches")
			{
				list = parseWatchData(xml);
				m_View.refreshWatches(list);	
			}
			else if (cmd == "message")
			{
				logMessage(xml.toString());	
			}
		}
		
		public function getInstructionPtrHighlight():Highlight
		{
			return m_InstructionPtrHighlight;	
		}
		
		private function parseWatchData(xml:XML):ArrayList
		{
			var list:ArrayList = new ArrayList();
			
			var items : XMLList = xml.elements();
			
			for each (var item : XML in items)
			{
				var watch:WatchItem = new WatchItem(item);
				list.addItem(watch);
			}
			
			return list;
		}
		
		
		private function parseHighlight(xml:XML):Highlight
		{
			var srcid:int = xml.@srcid;	
			var cf:int = xml.@cf;	
			var ct:int = xml.@ct;	
			var lf:int = xml.@lf;	
			var lt:int = xml.@lt;	
			
			if (m_Sources.hasOwnProperty(srcid))
			{			
				var src:SourceCode = m_Sources[srcid] as SourceCode;
				
				var from:int = src.flattenLocation(lf, cf);
				var to:int = src.flattenLocation(lt, ct);
				
				return new Highlight(src, from, to);
			}
			else
			{
				trace("defaulting to default highlight...");
				return null;	
			}
		}

		public function logMessage(text : String) : void
		{
			m_View.appendMessage(text);	
		}
		
		
		private function onFatalError(text : String) : void
		{
			logMessage(text);
			
			Alert.show("An error occurred while communicating with the scripting host.\n\nPress OK to reload and retry.\n\nError was:" + text, "Error", Alert.OK, m_View, function():void
			{
				ExternalInterface.call("document.location.reload", true);
			});
		}
		
		private function ioErrorHandler(event:IOErrorEvent):void {
			onFatalError("IO Error : " + event.text);
		}
		
		private function progressHandler(event:ProgressEvent):void {
			// trace("progressHandler loaded:" + event.bytesLoaded + " total: " + event.bytesTotal);
		}
		
		private function securityErrorHandler(event:SecurityErrorEvent):void {
			onFatalError("IO Error : " + event.text);
		}				
	
		public function refresh() : void
		{
			m_Socket.send(<Command cmd="refresh" />);		
		}
		
		public function stepIn() : void
		{
			m_Socket.send(<Command cmd="stepIn" />);		
		}
		
		public function stepOut() : void
		{
			m_Socket.send(<Command cmd="stepOut" />);		
		}
		
		public function stepOver() : void
		{
			m_Socket.send(<Command cmd="stepOver" />);		
		}
		
		public function run() : void
		{
			m_Socket.send(<Command cmd="run" />);		
		}
		
		public function addWatch(varNames : String) : void
		{
			var cmd:XML = <Command cmd="addWatch" />;
			cmd.@arg = varNames;
			m_Socket.send(cmd);			
		}
		
		public function removeWatch(varNames : String) : void
		{
			var cmd:XML = <Command cmd="delWatch" />;
			cmd.@arg = varNames;
			m_Socket.send(cmd);			
		}		
		
	}
}