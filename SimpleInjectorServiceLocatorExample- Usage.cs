
//Put this code in your Dependency registration
//@Todo: 1) Replace IAppender with your abstract type
//       2) Replace DatabaseAppender with your choice of key which application will be passing at run-time
//       3) Replace SampleDatabaseAppender with your type which you want to return at the time to resolving IAppender with key DatabaseAppender.
//       4) Since it's name value pair you can add more keys. e.g. { "WebApiAppender", typeof(SampleWebApiAppender) }

container.RegisterSingleton<JwtSts.IRequestHandlerFactory<IAppender>>(new RequestHandlerFactory<IAppender>(container) {
				{ "DatabaseAppender", typeof(SampleDatabaseAppender) }
				});	