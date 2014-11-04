package
{
	public final class WatchItem
	{
		[Bindable]
		public var Name : String;
		[Bindable]
		public var Value : String;
		[Bindable]
		public var Type : String;
		[Bindable]
		public var Address : String;
		[Bindable]
		public var BasePtr : String;
		[Bindable]
		public var RetAddress : String;
		[Bindable]
		public var LValue : String;
		
		
		public function WatchItem(xml :XML)
		{
			Name = xml.@name;
			Value = xml.@value;
			Type = xml.@type;
			Address = xml.@address;
			BasePtr = xml.@baseptr;
			RetAddress = xml.@retaddress;
			LValue = xml.@lvalue;
		}
		
	
	}
}