all: compile test

compile:
	gmcs @SharpExpress.rsp

test:
	gmcs -pkg:nunit /out:SharpExpress.test.dll /define:NUNIT @SharpExpress.rsp
	nunit-console SharpExpress.test.dll
