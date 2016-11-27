using NUnit.Framework;
using System;
using Tac.MetaServlet.V56.Client;
using NLog;
using Tac.MetaServlet.Rpc;
using Unclazz.Commons.Json;
using System.Net;

namespace Test.Tac.MetaServlet.V56.Client
{
	[TestFixture()]
	public class MainClassTest
	{
		private readonly Parameters ps;
		private readonly Context ctx;
		private readonly MockAgent agent;
		private readonly MainClass main;

		public MainClassTest()
		{
			ps = new Parameters();
			ps.Request.TaskName = "taskName";
			ps.Request.AuthUser = "authUser";
			ps.Request.AuthPass = "authPass";
			ctx = new Context();
			agent = new MockAgent();
			main = new MainClass(agent.DelegateAgent);
		}

		IRequest MakeRequest(Func<IRequest, IResponse> f)
		{
			return Request
				.Builder()
			   .Host("host")
			   .Port(1)
			   .Path("path")
			   .AuthUser("username")
			   .AuthPass("password")
			   .Timeout(1)
               .Agent(f)
               .Build();
		}

		/// <summary>
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0ならtaskIdを含んだJSONを返します。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ReturnsJsonIncludesTaskId_IfRemoteResponseOKAndReturnCode0()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
				                          (b) => b.Append("taskId", 123));
			};

			// Act
			var resp = main.RequestGetTaskIdByName(ps, ctx);

			// Assert
			Assert.That(resp.GetProperty("taskId").NumberValue(), Is.EqualTo(123));
		}

		/// <summary>
		/// APIレスポンスのHTTPステータスがOKでもreturnCodeが0以外なら例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfRemoteResponseOKAndReturnCode1()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("taskId", 123)
				                          .Append("returnCode", 1));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}

		/// <summary>
		/// APIレスポンスのHTTPステータスがOK以外ならreturnCodeが0でも例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfRemoteResponseNGAndReturnCode0()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.BadRequest, 0,
										  (b) => b.Append("taskId", 123)
										  .Append("returnCode", 0));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}

		/// <summary>
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0でも"taskId"が含まれない場合は例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfResponseDoesNotIncludeTaskId()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.BadRequest, 0,
										  (b) => b.Append("taskID", 123)
										  .Append("returnCode", 0));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}
	}
}
