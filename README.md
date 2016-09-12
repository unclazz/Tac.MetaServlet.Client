# Tac.MetaServlet.Client

このVisualStudioソリューションはTalend Administration Center（TAC）のRPCインターフェースであるMetaServletに対して
.NET FrameworkやMonoアプリケーションからアクセスすることを可能にするAPIを提供するものです。
C#のバージョンは5.0をターゲットとしています。

ソリューションとその成果物はMITライセンスのもとで公開しています。
これらはいずれもリポジトリのコミッターが個人的に製造しているものですので、
TACの提供元からのサポートやTACの仕様変更に対するAPIの追随の保証は一切ありません。

## Tac.MetaServlet.Client

後述の2つのプロジェクトのアセンブリを参照し、実際にTACに対するRPCを行うサンプル・アプリケーションです。
.NET Framework 4.5.2がインストールされた環境にて動作確認を行っています。
ビルドにより生成された*.exeファイルをコマンドライン引数なしで実行するとUSAGEが表示されます。
ご覧のとおりRPCリクエストの内容を表すJSON形式ファイルだけは指定が必須です。

```
構文: TACRPC /J <json-file> [/H <host>] [/P <port>] [/Q <path>] [/T <timeout>]
解説: /J  RPCリクエストを表わすJSONが記述されたファイルのパス.
      /H  RPCリクエスト先のホスト名. デフォルトは"localhost".
      /P  RPCリクエスト先のポート名. デフォルトは8080.
      /Q  RPCリクエスト先のパス名. デフォルトは"/org.talend.administrator/metaServlet".
      /T  RPCリクエストのタイムアウト時間. 単位はミリ秒. デフォルトは100000.
```

パラメータはコマンドライン引数だけでなく、アプリケーション構成ファイルでも指定することが出来ます。
コマンドライン引数とアプリケーション構成ファイル双方にパラメータの記載がある場合は、コマンドライン引数が優先されます。
したがってアプリケーション構成ファイルはコマンドライン引数が指定されない場合のフォールバックとして機能します。

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
        <add key="Request.Json" value="request.json" />
        <!--add key="Remote.Host" value="localhost" />
        <add key="Remote.Port" value="8080" />
        <add key="Remote.Path" value="/org.talend.administrator/metaServlet" />
        <add key="Request.Timeout" value="100000" /-->
    </appSettings>
</configuration>
```

## Tac.MetaServlet.Rpc

このプロジェクトはRPCリクエストの生成とレスポンスの解析のために直接的に必要になるインターフェースおよび
その実装とユーティリティを提供します。

RPCリクエストのロジックの起点となるのは`Request.Builder()`メソッドが返すビルダーです。
このビルダーを通じてリクエストに必要なメタ情報とリクエストの内容を指定して`IRequest`インターフェースのインスタンスを構築できます。

リクエストは内部的には`System.Net.WebRequest`クラスとそのサブクラスにより担われています。
このデフォルトの動作を変更したい場合には、前述のビルダーの`Agent(Func<IRequest, IResponse>)`メソッドを呼び出して、
代替となるHTTPリクエストのロジックを登録してください。

## Tac.MetaServlet.Json

このプロジェクトはRPCリクエストのパラメータおよびRPCレスポンスの本文を構成するJSON形式データにアクセスするための
インターフェースおよびその実装とユーティリティを提供します。

JSON形式データのパースは`JsonObject.FromFile(...)`や`JsonObject.FromString(...)`メソッドなどを通じて行います。
JSONをプログラム・ロジックにより構築する場合は`JsonObject.Of(...)`ファクトリ・メソッドや
`JsonObject.Builder()`メソッドを通じて得られるビルダーを使用してください。

ファイルやストリームからパースしたものであれプログラムにより構築したものであれ、
`IJsonObject`インターフェースが提供する`ToString()`メソッドや`Format(IJsonFormatOptions)`メソッドによって
JSONの文字列表現を得ることができます。
`IJsonFormatOptions`は`JsonFormatOptions.Builder()`メソッドが返すビルダーを使用して構築することができます。
