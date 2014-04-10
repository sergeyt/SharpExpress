all: test

get-nuget:
	bash get-nuget

get-deps: get-nuget
	echo 'getting dependencies'
	bash nuget install -OutputDirectory packages Moq

compile: get-nuget get-deps
	gmcs @SharpExpress.rsp

test: get-nuget get-deps
	gmcs -pkg:nunit /define:NUNIT @SharpExpress.rsp
	nunit-console SharpExpress.dll
