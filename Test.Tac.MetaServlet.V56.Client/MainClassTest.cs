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
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
		/// APIリクエストは"taskName"のほか必須のパラメータを含んでいます。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_SendRequest_ThatIncludesRequitedParameters()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				Assert.That(r.ActionName, Is.EqualTo("getTaskIdByName"));
				Assert.That(r.AuthUser, Is.EqualTo(ps.Request.AuthUser));
				Assert.That(r.AuthPass, Is.EqualTo(ps.Request.AuthPass));
				Assert.That(r.Timeout, Is.EqualTo(ps.Request.Timeout));
				Assert.That(r.Host, Is.EqualTo(ps.Remote.Host));
				Assert.That(r.Port, Is.EqualTo(ps.Remote.Port));
				Assert.That(r.Path, Is.EqualTo(ps.Remote.Path));
				Assert.That(r.Parameters["taskName"].StringValue(), Is.EqualTo(ps.Request.TaskName));

				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("taskId", 123));
			};

			// Act
			// Assert
			main.RequestGetTaskIdByName(ps, ctx);
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
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
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでもreturnCodeが0以外なら例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfRemoteResponseOKAndReturnCode1()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 1,
										  (b) => b.Append("taskId", 123));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
		/// APIレスポンスのHTTPステータスがOK以外ならreturnCodeが0でも例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfRemoteResponseNGAndReturnCode0()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.BadRequest, 0,
										  (b) => b.Append("taskId", 123));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0でも"taskId"が含まれない場合は例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskIdByName_ThrowsException_IfRemoteResponseDoesNotIncludeTaskId()
		{
			// Arrange
			agent.ResponseGetTaskIdByName = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("taskID", 123));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskIdByName(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskStatus"/>のテスト。
		/// APIリクエストは"taskId"のほか必須のパラメータを含んでいます。
		/// </summary>
		[Test()]
		public void RequestGetTaskStatus_SendRequest_ThatIncludesRequitedParameters()
		{
			// Arrange
			agent.ResponseGetTaskStatus = (r) =>
			{
				Assert.That(r.ActionName, Is.EqualTo("getTaskStatus"));
				Assert.That(r.AuthUser, Is.EqualTo(ps.Request.AuthUser));
				Assert.That(r.AuthPass, Is.EqualTo(ps.Request.AuthPass));
				Assert.That(r.Timeout, Is.EqualTo(ps.Request.Timeout));
				Assert.That(r.Host, Is.EqualTo(ps.Remote.Host));
				Assert.That(r.Port, Is.EqualTo(ps.Remote.Port));
				Assert.That(r.Path, Is.EqualTo(ps.Remote.Path));
				Assert.That(r.Parameters["taskId"].NumberValue(), Is.EqualTo(ctx.TaskId));

				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("status", "TESTING!"));
			};

			// Act
			// Assert
			main.RequestGetTaskStatus(ps, ctx);
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskStatus"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0ならstatusを含んだJSONを返します。
		/// </summary>
		[Test()]
		public void RequestGetTaskStatus_ReturnsJsonIncludesStatus_IfRemoteResponseOKAndReturnCode0()
		{
			// Arrange
			agent.ResponseGetTaskStatus = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("status", "TESTING!"));
			};

			// Act
			var resp = main.RequestGetTaskStatus(ps, ctx);

			// Assert
			Assert.That(resp.GetProperty("status").StringValue(), Is.EqualTo("TESTING!"));
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskStatus"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでもreturnCodeが0以外なら例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskStatus_ThrowsException_IfRemoteResponseOKAndReturnCode1()
		{
			// Arrange
			agent.ResponseGetTaskStatus = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 1,
										  (b) => b.Append("status", "TESTING!"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskStatus(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskStatus"/>のテスト。
		/// APIレスポンスのHTTPステータスがOK以外ならreturnCodeが0でも例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskStatus_ThrowsException_IfRemoteResponseNGAndReturnCode0()
		{
			// Arrange
			agent.ResponseGetTaskStatus = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.BadRequest, 0,
										  (b) => b.Append("status", "TESTING!"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskStatus(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestGetTaskIdByName"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0でも"status"が含まれない場合は例外をスローします。
		/// </summary>
		[Test()]
		public void RequestGetTaskStatus_ThrowsException_IfRemoteResponseDoesNotIncludeStatus()
		{
			// Arrange
			agent.ResponseGetTaskStatus = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("state", "TESTING!"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestGetTaskStatus(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestRunTask"/>のテスト。
		/// APIリクエストは"taskId"のほか必須のパラメータを含んでいます。
		/// </summary>
		[Test()]
		public void RequestRunTask_SendRequest_ThatIncludesRequitedParameters()
		{
			// Arrange
			agent.ResponseRunTask = (r) =>
			{
				Assert.That(r.ActionName, Is.EqualTo("runTask"));
				Assert.That(r.AuthUser, Is.EqualTo(ps.Request.AuthUser));
				Assert.That(r.AuthPass, Is.EqualTo(ps.Request.AuthPass));
				Assert.That(r.Timeout, Is.EqualTo(ps.Request.Timeout));
				Assert.That(r.Host, Is.EqualTo(ps.Remote.Host));
				Assert.That(r.Port, Is.EqualTo(ps.Remote.Port));
				Assert.That(r.Path, Is.EqualTo(ps.Remote.Path));
				Assert.That(r.Parameters["taskId"].NumberValue(), Is.EqualTo(ctx.TaskId));

				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("execRequestId", "TEST_123"));
			};

			// Act
			// Assert
			main.RequestRunTask(ps, ctx);
		}
		/// <summary>
		/// <see cref="MainClass.RequestRunTask"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0ならexecRequestIdを含んだJSONを返します。
		/// </summary>
		[Test()]
		public void RequestRunTask_ReturnsJsonIncludesStatus_IfRemoteResponseOKAndReturnCode0()
		{
			// Arrange
			agent.ResponseRunTask = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("execRequestId", "TEST_123"));
			};

			// Act
			var resp = main.RequestRunTask(ps, ctx);

			// Assert
			Assert.That(resp.GetProperty("execRequestId").StringValue(), Is.EqualTo("TEST_123"));
		}

		/// <summary>
		/// <see cref="MainClass.RequestRunTask"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでもreturnCodeが0以外なら例外をスローします。
		/// </summary>
		[Test()]
		public void RequestRunTask_ThrowsException_IfRemoteResponseOKAndReturnCode1()
		{
			// Arrange
			agent.ResponseRunTask = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 1,
										  (b) => b.Append("execRequestId", "TEST_123"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestRunTask(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestRunTask"/>のテスト。
		/// APIレスポンスのHTTPステータスがOK以外ならreturnCodeが0でも例外をスローします。
		/// </summary>
		[Test()]
		public void RequestRunTask_ThrowsException_IfRemoteResponseNGAndReturnCode0()
		{
			// Arrange
			agent.ResponseRunTask = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.BadRequest, 0,
										  (b) => b.Append("execRequestId", "TEST_123"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestRunTask(ps, ctx);
			});
		}

		/// <summary>
		/// <see cref="MainClass.RequestRunTask"/>のテスト。
		/// APIレスポンスのHTTPステータスがOKでreturnCodeが0でも"status"が含まれない場合は例外をスローします。
		/// </summary>
		[Test()]
		public void RequestRunTasks_ThrowsException_IfRemoteResponseDoesNotIncludeStatus()
		{
			// Arrange
			agent.ResponseRunTask = (r) =>
			{
				return agent.MakeResponse(r, HttpStatusCode.OK, 0,
										  (b) => b.Append("execRequestID", "TEST_123"));
			};

			// Act
			// Assert
			Assert.Throws<ClientException>(() =>
			{
				main.RequestRunTask(ps, ctx);
			});
		}
	}
}
