# Tac.MetaServlet.Client

このVisualStudioソリューションはTalend Administration Center（TAC）のRPCインターフェースであるMetaServletに対して.NET FrameworkやMonoアプリケーションからアクセスすることを可能にするAPIを提供するものです。C#のバージョンは5.0をターゲットとしています。

ソリューションとその成果物はMITライセンスのもとで公開しています。これらはいずれもリポジトリのコミッターが個人的に製造しているものですので、TACの提供元からのサポートやTACの仕様変更に対するAPIの追随の保証は一切ありません。

ソリューションにはクラス・ライブラリ`Tac.MetaServlet.Rpc`とそれを使用したサンプル・アプリ`Tac.MetaServlet.Client`の2つのプロジェクトが含まれています。前者のプロジェクトをビルドして作成したアセンブリは[NuGet Gallery](https://www.nuget.org/packages/Tac.MetaServlet.Rpc/)で公開されています。

## Tac.MetaServlet.Rpc

このプロジェクトはRPCリクエストの生成とレスポンスの解析のために直接的に必要になるインターフェースおよびその実装とユーティリティを提供します。

### IRequet (Tac.MetaServlet.Rpc.IRequest)

RPCリクエストを表わすインターフェースです。インターフェースを実装した具象クラスは`Request`で、`Request.Builder()`メソッドを通じて得られるビルダーを使用して構築することができます。リクエストとレスポンスの内容はJSON形式のデータで表されます。JSON形式データをC#オブジェクトとして扱うために[Unclazz.Commons.Json](https://github.com/unclazz/Unclazz.Commons.Json)アセンブリから提供されている`IJsonObject`や`JsonObjectBuilder`を用いています。

|メンバー|型|説明|
|---|---|---|
|Host|string|リクエスト先のホスト名|
|Port|int|リクエスト先のポート番号|
|Path|string|リクエスト先のコンテキストパス|
|Uri|System.Uri|エンコード済みのリクエスト内容を含むリクエストURI|
|Timeout|int|リクエストがタイムアウトするまでのミリ秒|
|ActionName|string|RPCリクエストするアクション名|
|AuthUser|string|RPCリクエストする認証ユーザ名|
|AuthPass|string|RPCリクエストする認証パスワード|
|Parameters|Unclazz.Commons.Json.IJsonObject|PRCリクエストのパラメータを表わすJSON|
|Send()|IResponse|RPCリクエストを送信する|
|SendAsync()|Task&lt;IResponse&gt;|RPCリクエストを非同期に送信する|

### IResponse (Tac.MetaServlet.Rpc.IResponse)

RPCレスポンスを表わすインターフェースです。インスタンスは`IRequest.Send()`や`IRequest.SendAsync()`を通じて得られます。インターフェースを実装した具象クラスは`Response`で、`Response.Builder()`メソッドを通じて得られるビルダーを使用して構築することもできます。

|メンバー|型|説明|
|---|---|---|
|Request|IRequest|このレスポンスが生成される元になったリクエスト|
|StatusCode|System.Net.HttpStatusCode|HTTPステータスコード|
|ReturnCode|int|RPCレスポンスのリターンコード|
|Body|Unclazz.Commons.Json.IJsonObject|RPCレスポンスの本文として返されたJSON|

### RequrstBuilder (Tac.MetaServlet.Rpc.RequrstBuilder)

RPCリクエストのロジックの起点となるオブジェクトです。このビルダーを通じてリクエストに必要なメタ情報とリクエストの内容を指定して`IRequest`インターフェースのインスタンスを構築できます。

リクエストのHTTP通信部分は内部的には`System.Net.WebRequest`クラスとそのサブクラスにより担われています。このデフォルトの動作を変更したい場合には、前述のビルダーの`Agent(Func<IRequest, IResponse>)`メソッドを呼び出して、代替となるHTTPリクエストのロジックを登録してください。

## Tac.MetaServlet.Client

前述のプロジェクトのアセンブリを参照し、実際にTACに対するRPCを行うサンプル・アプリケーションです。.NET Framework 4.5.2がインストールされた環境にて動作確認を行っています。ビルドにより生成された`*.exe`ファイルをコマンドライン引数なしで実行するとヘルプが表示されます。ご覧のとおりRPCリクエストの内容を表すJSON形式ファイルだけは指定が必須です。

```
構文: TACRPC {/J <json-file> | /?} [/H <host>] [/P <port>] [/Q <path>] [/T <timeout>] [/D]
      /J  RPCリクエストを表わすJSONが記述されたファイルのパス.
      /H  RPCリクエスト先のホスト名. デフォルトは"localhost".
      /P  RPCリクエスト先のポート名. デフォルトは8080.
      /Q  RPCリクエスト先のパス名. デフォルトは"/org.talend.administrator/metaServlet".
      /T  RPCリクエストのタイムアウト時間. 単位はミリ秒. デフォルトは100000.
      /D  リクエストとレスポンスのダンプ出力を行う.
      /?  このヘルプを表示する.
```

パラメータはコマンドライン引数だけでなく、アプリケーション構成ファイルでも指定することが出来ます。コマンドライン引数とアプリケーション構成ファイル双方にパラメータの記載がある場合は、コマンドライン引数が優先されます。したがってアプリケーション構成ファイルはコマンドライン引数が指定されない場合のフォールバックとして機能します。

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
        <!--add key="Tac.MetaServlet.Client.Request.Json" value="request.json" />
        <add key="Tac.MetaServlet.Client.Remote.Host" value="localhost" />
        <add key="Tac.MetaServlet.Client.Remote.Port" value="8080" />
        <add key="Tac.MetaServlet.Client.Remote.Path" value="/org.talend.administrator/metaServlet" />
        <add key="Tac.MetaServlet.Client.Request.Timeout" value="100000" / -->
    </appSettings>
</configuration>
```

## Tac.MetaServlet.V56.Client

TAC v5.6もしくはそれ以上を対象にしたタスク実行専用のコンソール・アプリケーションです。サンプル・アプリケーション`Tac.MetaServlet.Client`同様に、.NET Framework 4.5.2がインストールされた環境にて動作確認を行っています。

ビルドにより生成された`tacrpc.v56.exe`ファイルをコマンドライン引数なしで実行するとヘルプが表示されます：

```
Syntax:
                    tacrpc.v56.exe /a <user> /b <password> /n <task> [/dryrun] [
                    /h <hostname>] [/i <interval>] [/j <instance>] [/l <filename
                    >] [/p <port>] [/q <path>] [/t <timeout>] [/u <timeout>]

Description:
                    A RPC client command to execute task on TAC(Talend Administr
                    ation Center).

Options:
/a, /authuser       Username for authentication of API access.
/b, /authpass       Password for authentication of API access.
/dryrun             Use mock for a simulation. Request is NOT sent for anything.
                    
/h                  Hostname of API. Default is "/org.talend.administrator/metaS
                    ervlet".
/i                  Interval for executing API request. Specify value by seconds
                    . Default is 60.
/j                  Name of command instance. Default is "tacrpc".
/l                  Name of log file. Default is "tacrpc_${var:instanceName}_${v
                    ar:executionName}_${var:yyyyMMdd}_${var:hhmmssfff}.log".
/n                  Task name to execute.
/p                  Port number of API. Default is 8080.
/q, /path           Context path of API. Default is "/org.talend.administrator/m
                    etaServlet".
/t                  Timeout for executing API request. Specify value by seconds.
                     Default is 60.
/u                  Timeout for executing THIS command.Specify value by seconds.
                     Default is 3600.
```

バッチ・プログラムとして起動されることを想定しており、たくさんのオプションが用意されています。これらのオプションのほとんどすべてはアプリケーション構成ファイルでも指定できるようになっています：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<appSettings>
        <add key="Tac.MetaServlet.V56.Client.Remote.Host" value="localhost"/>
		<add key="Tac.MetaServlet.V56.Client.Remote.Port" value="8080"/>
		<add key="Tac.MetaServlet.V56.Client.Remote.Path" value="/org.talend.administrator/metaServlet"/>
		<add key="Tac.MetaServlet.V56.Client.Request.TaskName" value="example_task"/>
		<add key="Tac.MetaServlet.V56.Client.Request.Interval" value="60"/>
		<add key="Tac.MetaServlet.V56.Client.Request.Timeout" value="60"/>
		<add key="Tac.MetaServlet.V56.Client.Request.AuthUser" value="user@example.com"/>
		<add key="Tac.MetaServlet.V56.Client.Request.AuthPass" value="password"/>
		<add key="Tac.MetaServlet.V56.Client.Execution.LogFileName" value="tacrpc_${var:instanceName}_${var:executionName}_${var:yyyyMMdd}_${var:hhmmssfff}.log"/>
		<add key="Tac.MetaServlet.V56.Client.Execution.InstanceName" value="tacrpc"/>
		<add key="Tac.MetaServlet.V56.Client.Execution.Timeout" value="3600"/>
	</appSettings>
</configuration>
```

`tacrpc.v56.exe`はおおよそ以下の順序で`runTask`リクエストを行います：

1. `getTaskIdByName`リクエストを行い`taskId`を解決する。解決できない場合は処理を中断する。
2. `getTaskStatus`リクエストを行い`status: "READY_TO_RUN"`であることを確認する。`status`が異なるときは処理を中断する。
3. `runTask`リクエストを`mode: "asynchronous"`指定で行う。
4. `getTaskExecutionStatus`リクエストを一定間隔ごとに行い`jobExitCode`が返されるまでポーリングする。制限時間を超過した場合は処理を中断する。
5. `taskLog`リクエストを行い直近のタスク実行時のログをダウンロードする。

`tacrpc.v56.exe`の終了コードは以下のルールで決まります。リストのより下にあるもの（より条件の厳しいもの）が優先されます：

- APIリクエスト時の通信障害、制限時間超過などがあった場合、終了コードは`1`となる。
- APIレスポンスの`IResponse#StatusCode`が`OK`以外の場合、終了コードは`1`となる。
- APIレスポンスの`IResponse#ReturnCode`が`0`以外の場合、終了コードもそれと等しい値となる。
- `getTaskStatus`リクエストに対するレスポンスの`status`が`"READY_TO_RUN"`でない場合、終了コードは`1`となる。
- `runTask`リクエストに対するレスポンスの`jobExitCode`が`0`以外の場合、終了コードもそれと等しい値となる。
- 以上のいずれにも該当しないときのみ`0`となる。
