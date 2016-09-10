using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Tac.MetaServlet.Json.Parser
{
	/// <summary>
	/// パーサーの入力となるオブジェクトです。
	/// 文字列やファイル入力ストリームをラップします。
	/// EOFに到達するか処理中にIOエラーが発生した場合は自動でリソースを解放します。
	/// </summary>
	public sealed class Input : IDisposable
	{
		/// <summary>
		/// 文字列から入力オブジェクトのインスタンスを生成します。
		/// </summary>
		/// <param name="s">文字列</param>
		/// <returns>インスタンス</returns>
		public static Input FromString(string s)
		{
			return new Input(new StringReader(s));
		}
		/// <summary>
		/// ファイルから入力オブジェクトのインスタンスを生成します。
		/// </summary>
		/// <param name="path">ファイルのパス</param>
		/// <param name="enc">エンコーディング</param>
		/// <returns>インスタンス</returns>
		public static Input FromFile(string path, Encoding enc)
		{
			return new Input(new StreamReader(path, enc));
		}
		/// <summary>
		/// ファイルから入力オブジェクトのインスタンスを生成します。
		/// エンコーディングにはシステムのデフォルトのエンコーディングが使用されます。
		/// </summary>
		/// <param name="path">ファイルのパス</param>
		/// <returns>インスタンス</returns>
		public static Input FromFile(string path)
		{
			return new Input(new StreamReader(path));
		}
		/// <summary>
		/// ストリームから入力オブジェクトのインスタンスを生成します。
		/// </summary>
		/// <param name="stream">ストリーム</param>
		/// <param name="enc">エンコーディング</param>
		/// <returns>インスタンス</returns>
		public static Input FromStream(Stream stream, Encoding enc)
		{
			return new Input(new StreamReader(stream, enc));
		}
		/// <summary>
		/// ストリームから入力オブジェクトのインスタンスを生成します。
		/// エンコーディングにはシステムのデフォルトのエンコーディングが使用されます。
		/// </summary>
		/// <param name="stream">ストリーム</param>
		/// <returns>インスタンス</returns>
		public static Input FromStream(Stream stream)
		{
			return new Input(new StreamReader(stream));
		}

		private static readonly int CR = '\r';
		private static readonly int LF = '\n';
		private static readonly char Null = '\u0000';

		private readonly TextReader reader;
		private readonly StringBuilder lineBuff = new StringBuilder();
		private int position = -1;
		private bool closed = false;

		/// <summary>
		/// 行番号
		/// </summary>
		public int LineNumber { get; private set; }
		/// <summary>
		/// 行内の位置（<code>1</code>始まり）
		/// </summary>
		public int ColumnNumber
		{
			get
			{
				return position + 1;
			}
		}
		/// <summary>
		/// 現在位置から行末までの文字列
		/// </summary>
		public string RestOfLine
		{
			get
			{
				int count = lineBuff.Length - position;
				char[] restBuff = new char[count];
				lineBuff.CopyTo(position, restBuff, 0, count);
				return new string(restBuff);
			}
		}
		/// <summary>
		/// 現在位置の文字
		/// </summary>
		public char Current { get; private set; }
		/// <summary>
		/// EOFに到達している場合<code>true</code>
		/// </summary>
		public bool EndOfFile { get; private set; }
		/// <summary>
		/// EOLに到達している場合<code>true</code>
		/// </summary>
		public bool EndOfLine
		{
			get
			{
				return EndOfFile || Current == CR || Current == LF;
			}
		}

		private Input(TextReader r)
		{
			LineNumber = 0;
			Current = Null;
			reader = r;
			GoNext();
		}

		/// <summary>
		/// 現在位置の文字が期待通りのものかチェックする.
		/// チェック結果がNGである場合、<see cref="ParseException"/>をスローする.
		/// </summary>
		/// <param name="expected">期待される文字.</param>
		/// <exception cref="ParseException">チェック結果がNGである場合</exception>
		public void Check(char expected)
		{
			char actual = Current;
			if (actual != expected)
			{
				throw new ParseException(this, string.Format
					("Syntax error. \"{0}\" expected but \"{1}\" found.", expected, actual));
			}
		}
		/// <summary>
		/// 空白文字をスキップする.
		/// <para>現在位置の文字およびそれに後続する文字が空白文字に該当する場合それらの文字を一括してスキップする.
		/// スキップ後現在位置は空白文字の次の非空白文字を指す.
		/// このメソッドが空白文字とみなすのはコードポイントが32（半角スペース）以下のすべての文字である.
		/// このメソッドはまた行コメントとブロックコメントもスキップする.</para>
		/// </summary>
		public void SkipWhitespace()
		{
			while (!EndOfFile)
			{
				if (Current <= ' ')
				{
					GoNext();
				}
				else if (Current == '/')
				{
					SkipComment();
				}
				else
				{
					return;
				}
			}
		}
		/// <summary>
		/// 行コメントおよびブロックコメントをスキップする.
		/// </summary>
		public void SkipComment()
		{
			Check('/');
			GoNext();
			if (Current == '/')
			{
				GoNextLine();
				return;
			}
			else if (Current == '*')
			{
				GoNext();
				while (!EndOfFile)
				{
					int p = RestOfLine.IndexOf("*/", StringComparison.CurrentCulture);
					if (p == -1)
					{
						GoNextLine();
					}
					else {
						GoNext(p + 2);
						return;
					}
				}
				throw new ParseException(this, "unclosed comment block.");
			}
			throw new ParseException(this, string.
			Format("'/' or '*' expected but {0} found.", Current));
		}
		/// <summary>
		/// 引数で指定されたワードを読み取りその分現在位置を前進させる.
		/// このメソッドは読み取り前の現在位置からはじまり行末で区切られた文字列に対して、
		/// 引数で指定されたワードによる前方一致検索を行う.
		/// そして検索が失敗した場合は<see cref="ParseException"/>をスローする.
		/// </summary>
		/// <returns>前進後の現在位置の文字.</returns>
		/// <param name="keyword">読み取り対象のワード.</param>
		public char GoNext(string keyword)
		{
			if (RestOfLine.StartsWith(keyword, StringComparison.CurrentCulture))
			{
				GoNext(keyword.Length);
				return Current;
			}
			throw new ParseException(this, string.Format("keyword \"{0}\" is not found.", keyword));
		}
		/// <summary>
		/// 引数で指定された正規表現パターンにマッチする文字列を読み取りその分現在位置を前進させる.
		/// このメソッドは読み取り前の現在位置からはじまり行末で区切られた文字列に対して、
		/// 引数で指定された正規表現パターンによる前方一致検索を行う.
		/// そして検索が失敗した場合は<see cref="ParseException"/>をスローする.
		/// </summary>
		/// <returns>読み取り結果.</returns>
		/// <param name="regex">読み取り対象の文字列を表わす正規表現パターン.</param>
		public string ClipToken(Regex regex)
		{
			Match m = regex.Match(RestOfLine);
			if (m.Success && m.Index == 0)
			{
				string r = m.Value;
				GoNext(r.Length);
				return r;
			}
			throw new ParseException
			(this, string.Format("sequence that matches the pattern \"{0}\" is not found.", regex));
		}
		/// <summary>
		/// 現在位置を引数で指定された文字数分前進させてその位置にある文字を返す.
		/// </summary>
		/// <returns>前進後の現在位置の文字.</returns>
		/// <param name="times">現在位置を前進させる文字数.</param>
		public char GoNext(int times)
		{
			for (int i = 0; i < times; i++)
			{
				GoNext();
			}
			return Current;
		}
		/// <summary>
		/// 現在位置を次の行の先頭に移動させその位にある文字を返す.
		/// </summary>
		/// <returns>前進後の現在位置の文字.</returns>
		public char GoNextLine()
		{
			GoNext(RestOfLine.Length);
			return Current;
		}
		/// <summary>
		/// 現在位置を1つ前進させてその位置にある文字を返す.
		/// </summary>
		/// <returns>前進後の現在位置の文字</returns>
		public char GoNext()
		{
			// EOF到達後ならすぐに現在文字（ヌル文字）を返す
			if (EndOfFile)
			{
				return Current;
			}
			// EOF到達前なら次の文字を取得する処理に入る
			// まず現在位置をインクリメント
			position += 1;
			// 現在位置が行バッファの末尾より後方にあるかチェック
			if (position >= lineBuff.Length)
			{
				// ストリームの状態をチェック
				if (closed)
				{
					// すでにストリームが閉じられているならEOF
					// フラグを立て、キャッシュを後始末
					EndOfFile = true;
					Current = Null;
					position = 0;
				}
				else
				{
					// まだオープン状態なら次の行をロードする
					LoadLine();
				}
			}
			// EOFの判定を実施
			if (EndOfFile)
			{
				// EOF到達済みの場合
				// 現在文字（ヌル文字）を返す
				return Current;
			}
			else
			{
				// EOF到達前の場合
				// 現在文字に新しく取得した文字を設定
				Current = lineBuff[position];
				// 現在文字を返す
				return Current;
			}
		}

		private void LoadLine()
		{
			try
			{
				// 現在位置を初期化
				position = 0;
				LineNumber += 1;
				// 行バッファをクリアする
				lineBuff.Clear();
				// 繰り返し処理
				while (!closed)
				{
					// 次の文字を取得
					int c0 = reader.Read();
					// 文字のコード値が-1であるかどうか判定
					if (c0 == -1)
					{
						// 文字のコード値が−1ならストリームの終了
						// ストリームを即座にクローズする
						closed = true;
						reader.Dispose();
						// バッファが空ならEOFでもある
						EndOfFile = lineBuff.Length == 0;
						if (EndOfFile)
						{
							Current = Null;
						}
						return;
					}
					// 読み取った文字をバッファに格納
					lineBuff.Append((char)c0);
					// 文字がLF・CRであるかどうか判定
					if (c0 == LF)
					{
						// LFであればただちに読み取りを完了
						return;
					}
					else if (c0 == CR)
					{
						int c1 = reader.Peek();
						// 文字のコード値を判定
						if (c1 == -1)
						{
							// 文字のコード値が−1ならストリームの終了
							// ストリームを即座にクローズする
							closed = true;
							reader.Dispose();
						}
						else if (c1 == LF)
						{
							// LFであればそれもバッファに格納
							// 読み取り位置も前進させる
							lineBuff.Append((char)reader.Read());
						}
						// 読み取りを完了
						return;
					}
				} // End of while loop.
			}
			catch (IOException e)
			{
				reader.Dispose();
				throw new ParseException(this, "io error.", e);
			}
		}

		public override string ToString()
		{
			return string.Format("Input(Source={0},LineNumber={1},ColumnNumer={2})",
				reader, LineNumber, ColumnNumber);
		}

		public void Dispose()
		{
			reader.Dispose();
		}
	}
}

