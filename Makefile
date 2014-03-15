all: test

compile:
	gmcs @SharpExpress.rsp

test:
	gmcs -pkg:nunit /define:NUNIT @SharpExpress.rsp
	nunit-console SharpExpress.dll
