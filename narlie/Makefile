MCS = gmcs
MONO = mono
COMPILER_FRONTEND_ASSEMBLY = narc.exe
COMPILER_ASSEMBLY = Narlie.Compiler.dll
RUNTIME_ASSEMBLY = Narlie.Runtime.dll

MONO_CODEGENERATION_SOURCES = \
	$(wildcard Narlie.Compiler/Mono.CodeGeneration/*.cs)

NARLIE_COMPILER_SOURCES = \
	$(wildcard Narlie.Compiler/Narlie.Compiler.Symbols/*.cs) \
	$(wildcard Narlie.Compiler/Narlie.Compiler.CodeParser/*.cs) \
	$(wildcard Narlie.Compiler/Narlie.Compiler.CodeGen/*.cs) \
	$(wildcard Narlie.Compiler/Narlie.Compiler/*.cs)

NARLIE_COMPILER_NARC_SOURCES = \
	$(wildcard Narlie.Compiler.Narc/Narlie.Compiler.Narc/*.cs)

NARLIE_COMPILER_SOURCES_ALL = \
	$(MONO_CODEGENERATION_SOURCES) \
	$(NARLIE_COMPILER_SOURCES)

NARLIE_RUNTIME_SOURCES = \
	$(wildcard Narlie.Runtime/Narlie.Runtime/*.cs)

all: $(RUNTIME_ASSEMBLY) $(COMPILER_ASSEMBLY) $(COMPILER_FRONTEND_ASSEMBLY) 

$(COMPILER_ASSEMBLY): $(NARLIE_COMPILER_SOURCES_ALL) $(RUNTIME_ASSEMBLY)
	$(MCS) -out:$@ -debug -target:library -r:$(RUNTIME_ASSEMBLY) $(NARLIE_COMPILER_SOURCES_ALL)

$(RUNTIME_ASSEMBLY): $(NARLIE_RUNTIME_SOURCES)
	$(MCS) -out:$@ -debug -target:library $(NARLIE_RUNTIME_SOURCES)

$(COMPILER_FRONTEND_ASSEMBLY): $(NARLIE_COMPILER_NARC_SOURCES) $(COMPILER_ASSEMBLY)
	$(MCS) -out:$@ -debug -r:$(COMPILER_ASSEMBLY) $(NARLIE_COMPILER_NARC_SOURCES)

clean:
	rm -f $(COMPILER_ASSEMBLY){,.mdb} $(RUNTIME_ASSEMBLY){,.mdb} $(COMPILER_FRONTEND_ASSEMBLY){,.mdb} out.{exe,il} Narlie.Compiler/*.pidb Narlie.Runtime/*.pidb Narlie.Compiler.Narc/*.pidb

run: $(COMPILER_FRONTEND_ASSEMBLY)
	$(MONO) --debug $< < tests/clock.lisp
	monodis out.exe > out.il

count-compiler-lines:
	@wc -l $(NARLIE_COMPILER_SOURCES) | sort -n

count-runtime-lines:
	@wc -l $(NARLIE_RUNTIME_SOURCES) | sort -n

count-all-lines:
	@wc -l $(NARLIE_COMPILER_SOURCES_ALL) $(NARLIE_RUNTIME_SOURCES) | sort -n


