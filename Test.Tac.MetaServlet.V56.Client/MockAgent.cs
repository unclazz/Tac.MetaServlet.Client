using System;
using System.Net;
using Tac.MetaServlet.Rpc;
using Unclazz.Commons.Json;

namespace Test.Tac.MetaServlet.V56.Client
{
	public class MockAgent
	{
		public Func<IRequest, IResponse> ResponseGetTaskIdByName { get; set; }
		public Func<IRequest, IResponse> ResponseGetTaskStatus { get; set; }
		public Func<IRequest, IResponse> ResponseRunTask { get; set; }
		public Func<IRequest, IResponse> ResponseGetTaskExecutionStatus { get; set; }
		public Func<IRequest, IResponse> ResponseTaskLog { get; set; }

		public MockAgent()
		{
			ResponseGetTaskIdByName = (req) =>
			{
				return MakeResponse(req, HttpStatusCode.OK, 0,
				                    (b) => b.Append("taskId", 123));
			};
			ResponseGetTaskStatus = (req) =>
			{
				return MakeResponse(req, HttpStatusCode.OK, 0,
				                    (b) => b.Append("status", "READY_TO_RUN"));
			};
			ResponseRunTask = (req) =>
			{
				return MakeResponse(req, HttpStatusCode.OK, 0,
									(b) => b.Append("execRequestId", "123_abc"));
			};
			ResponseGetTaskExecutionStatus = (req) => 
			{
				return MakeResponse(req, HttpStatusCode.OK, 0,
				                    (b) => b.Append("jobExitCode", 0));
			};
			ResponseTaskLog = (req) =>
			{
				return MakeResponse(req, HttpStatusCode.OK, 0);
			};
		}

		public IResponse MakeResponse(IRequest req,
		                              HttpStatusCode status,
		                              int returnCode,
		                              Action<JsonObjectBuilder> modifier)
		{
			var b = Response
			   .Builder()
			   .Request(req)
				.StatusCode(status);
			var b2 = JsonObject
				.Builder()
				.Append("returnCode", returnCode);
			if (modifier != null) modifier(b2);
			return b.Body(b2.Build()).Build();
		}

		public IResponse MakeResponse(IRequest req,
									  HttpStatusCode status,
									  int returnCode)
		{
			return MakeResponse(req, status, returnCode, null);
		}

		public IResponse DelegateAgent(IRequest req)
		{
			if (req.ActionName.Equals("getTaskIdByName"))
			{
				return ResponseGetTaskIdByName(req);
			}
			else if (req.ActionName.Equals("getTaskStatus"))
			{
				return ResponseGetTaskStatus(req);
			}
			else if (req.ActionName.Equals("runTask"))
			{
				return ResponseRunTask(req);
			}
			else if (req.ActionName.Equals("getTaskExecutionStatus"))
			{
				return ResponseGetTaskExecutionStatus(req);
			}
			else if (req.ActionName.Equals("taskLog"))
			{
				return ResponseTaskLog(req);
			}
			else {
				return null;
			}
		}
	}
}
