using System;
using Newtonsoft.Json;

namespace PagarMe.Mpos
{
	public struct TerminalData<T>
	{
		[JsonProperty("timestamp")]
		public string CurrentVersion { get; set; }

		[JsonProperty("data")]
		public T[] Data { get; set; }
	}
}

