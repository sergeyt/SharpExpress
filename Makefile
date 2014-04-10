all: test

get-deps:
	bash get-nuget
	echo 'getting dependencies'
	bash nuget install -OutputDirectory packages Moq

compile: get-deps
	gmcs @SharpExpress.rsp

test: get-deps
	gmcs -pkg:nunit /define:NUNIT @SharpExpress.rsp
	nunit-console SharpExpress.dll
