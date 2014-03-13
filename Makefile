all: compile compile-test test

compile:
	gmcs @SharpExpress.rsp

compile-test:
	gmcs @SharpExpress.test.rsp

test:
	nunit-console SharpExpress.test.dll 

