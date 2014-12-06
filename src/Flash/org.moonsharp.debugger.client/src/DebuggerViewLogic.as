package
{
	import flash.display.LoaderInfo;
	import flash.events.*;
	import flash.external.ExternalInterface;
	import flash.net.XMLSocket;
	import flash.system.Security;
	import flash.system.System;
	import flash.utils.Dictionary;
	
	import mx.collections.ArrayList;
	import mx.controls.Alert;
	import mx.controls.List;
	import mx.core.FlexGlobals;
	import mx.managers.BrowserManager;
	import mx.managers.IBrowserManager;
	import mx.utils.URLUtil;

	public class DebuggerViewLogic
	{
		private var m_View : Main;
		private var m_Socket : XMLSocket;
		
		private var m_Sources : Dictionary = new Dictionary();
		private var m_SourceList : ArrayList = new ArrayList();
		
		private var m_InstructionPtrHighlight : Highlight = null;
		
		private var m_ErrorRx:String;
		
		
		public function DebuggerViewLogic(view : Main, loaderInfo: LoaderInfo)
		{
			m_View = view;
			
			var domain:String = getDomain();
			var port:int = 1;
			
			if (domain == null)
			{
				domain = "127.0.0.1";
				port = 2006;
				logMessage("Running under Flex debugger ? Assuming default host/port.");
			}
			else
			{
				var portstr:String = FlexGlobals.topLevelApplication.parameters.port;
				
				logMessage(portstr);
				port = int(portstr);
			}
			
			logMessage("Connecting to: " + domain + ":" + port);
			
			m_Socket = new XMLSocket(domain, port);
			
			m_Socket.addEventListener(Event.CLOSE, closeHandler);
			m_Socket.addEventListener(Event.CONNECT, connectHandler);
			m_Socket.addEventListener(DataEvent.DATA, dataHandler);
			m_Socket.addEventListener(IOErrorEvent.IO_ERROR, ioErrorHandler);
			m_Socket.addEventListener(ProgressEvent.PROGRESS, progressHandler);
			m_Socket.addEventListener(SecurityErrorEvent.SECURITY_ERROR, securityErrorHandler);
		} 
		
		public function getDomain():String
		{
			var domain:String = Security.pageDomain;
			if (domain == null) return null;

			return URLUtil.getServerName(domain);
		}
		
		
		
		private function closeHandler(event:Event):void {
			onFatalError("Connection closed.");
		}
		
		private function connectHandler(event:Event):void {
			trace("connectHandler: " + event);
			logMessage("Connection with host established.");
			m_Socket.send(<Command cmd="handshake" />);		
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
			else if (cmd == "breakpoints")
			{
				refreshBreakpoints(xml);
			}
			else if (cmd == "error_rx")
			{
				m_ErrorRx = xml.@arg;	
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
				logMessage("ERROR: Highlighted source " + srcid.toString() + " not found in sources list.");
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
		
		private function refreshBreakpoints(xml : XML): void
		{
			for(var i:int = 0; i < m_SourceList.length; i++)
				m_SourceList.getItemAt(i).Breakpoints= new Vector.<Highlight>();
			
			for each(var x:XML in xml.elements())
			{
				var hl:Highlight = parseHighlight(x);
				
				if (hl != null)
					hl.Source.Breakpoints.push(hl);	
			}
			
			m_View.refreshBreakpoints();
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
		
		public function pause() : void
		{
			m_Socket.send(<Command cmd="pause" />);		
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
		
		
		public function toggleBreakpoint(src:int, line:int, col:int, action:String) : void
		{
			var cmd:XML = <Command cmd="breakpoint" />;
			cmd.@arg = action;
			cmd.@src = src;
			cmd.@line = line;
			cmd.@col = col;
			m_Socket.send(cmd);		
		}	
		
		public function getErrorRx():String
		{
			return m_ErrorRx;	
		}
		
		public function setErrorRx(val:String):void
		{
			m_ErrorRx = val;	
			var cmd:XML = <Command cmd="error_rx" />;
			cmd.@arg = val;
			m_Socket.send(cmd);
		}
		
		
		
	}
}