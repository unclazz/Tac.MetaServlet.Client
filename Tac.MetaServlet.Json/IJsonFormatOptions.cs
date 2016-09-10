using System;
namespace Tac.MetaServlet.Json
{
	/// <summary>
	/// JSONの整形方法を指定するオプションを表わすインターフェースです.
	/// </summary>
	public interface IJsonFormatOptions
	{
		/// <summary>
		/// インデントを行うかどうか.
		/// </summary>
		/// <value><c>true</c>の場合 インデントを行う.</value>
		bool Indent { get; }
		/// <summary>
		/// 改行文字として使用される文字列.
		/// </summary>
		/// <value>改行文字.</value>
		string NewLine { get; }
		/// <summary>
		/// インデントでソフトタブを使用するかどうか.
		/// </summary>
		/// <value><c>true</c>の場合 ソフトタブを使用する.</value>
		bool SoftTabs { get; }
		/// <summary>
		/// ソフトタブを使用するときのタブ幅.
		/// </summary>
		/// <value>タブ幅.</value>
		int TabWidth { get; }
	}
}

