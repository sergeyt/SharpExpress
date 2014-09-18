all: test

get-deps:
	bash get-nuget
	echo 'getting dependencies'
	# TODO use packages.config
	bash nuget install Moq
	cp ./Moq.4.2.1409.1722/lib/net35/Moq.dll ./Moq.dll

compile: get-deps
	gmcs @SharpExpress.rsp

test: get-deps
	gmcs -pkg:nunit /define:NUNIT @SharpExpress.rsp
	nunit-console SharpExpress.dll
