cellc-cs:
	make -s clean
	bin/build-runtime-src-file.py src/ tmp/runtime.cell
	bin/cellc-cs projects/compiler.txt tmp/
	bin/apply-hacks < tmp/generated.cs > dotnet/cellc-cs/generated.cs
	cp tmp/runtime.cs dotnet/cellc-cs/
	dotnet build -c Release dotnet/cellc-cs/

copy-cellc-cs:
	rm -f bin/cellc-cs bin/cellc-cs.dll
	cp dotnet/cellc-cs/bin/Release/netcoreapp3.1/cellc-cs bin/
	cp dotnet/cellc-cs/bin/Release/netcoreapp3.1/cellc-cs.dll bin/

cellcd-cs:
	make -s clean
	bin/cellc-cs -d projects/compiler-no-runtime.txt tmp/
	bin/apply-hacks < tmp/generated.cs > dotnet/cellcd-cs/generated.cs
	cp tmp/runtime.cs dotnet/cellcd-cs/
	dotnet build -c Debug dotnet/cellcd-cs/

compiler-test-loop:
	make -s clean
	bin/build-runtime-src-file.py src/ tmp/runtime.cell
	bin/cellc-cs projects/compiler.txt tmp/
	bin/apply-hacks < tmp/generated.cs > dotnet/cellc-cs/generated.cs
	cp tmp/runtime.cs dotnet/cellc-cs/
	dotnet build -c Release dotnet/cellc-cs/

	rm -rf tmp/*.cs
	dotnet run -c Release --project dotnet/cellc-cs/ projects/compiler.txt tmp/
	rm -rf dotnet/cellc-cs/generated.cs dotnet/cellc-cs/runtime.cs dotnet/cellc-cs/bin/ dotnet/cellc-cs/obj/
	cp tmp/generated.cs generated.cs
	bin/apply-hacks < tmp/generated.cs > dotnet/cellc-cs/generated.cs
	cp tmp/runtime.cs dotnet/cellc-cs/
	dotnet build -c Release dotnet/cellc-cs/

	rm -rf tmp/*.cs
	dotnet run -c Release --project dotnet/cellc-cs/ projects/compiler.txt tmp/
	cmp tmp/generated.cs generated.cs

clean:
	@rm -rf generated.cs tmp/ debug/
	@rm -rf dotnet/cellc-cs/generated.cs dotnet/cellc-cs/runtime.cs dotnet/cellc-cs/bin/ dotnet/cellc-cs/obj/
	@rm -rf dotnet/cellcd-cs/generated.cs dotnet/cellcd-cs/runtime.cs dotnet/cellcd-cs/bin/ dotnet/cellcd-cs/obj/
	@mkdir tmp/ debug/
