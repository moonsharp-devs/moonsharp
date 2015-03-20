# nmake /F makefile.mak

LUAJIT = luajit.exe
LUA = lua.exe
LUAC = luac.exe

RUN_LUA = $(LUA)
RUN_LUAC = $(LUAC)
OSNAME = MSWin32
ARCHNAME = MSWin32-x86-multi-thread
INTSIZE = 4

harness: env
	@prove --exec=$(LUA) *.t

sanity: env
	@prove --exec=$(LUA) 0*.t

luajit: env
	@prove --exec=$(LUAJIT) *.t

llvm-lua: env
	@prove --exec=$(LLVM_LUA) *.t

env:
	@set LUA_PATH=;;../src/?.lua
	@set LUA_INIT=platform = { lua=[[$(RUN_LUA)]], luac=[[$(RUN_LUAC)]], osname=[[$(OSNAME)]], intsize=$(INTSIZE), compat=true }

upload_pl = \
use strict; \
use warnings; \
use LWP::UserAgent; \
my $$ua = LWP::UserAgent->new(); \
$$ua->env_proxy(); \
my $$server = q{http://smolder.parrot.org}; \
my $$project_id = 7; \
my $$url = $$server . q{/app/projects/process_add_report/} . $$project_id; \
my $$response = $$ua->post( \
    $$url, \
    Content_Type => q{form-data}, \
    Content      => [ \
        architecture => q{$(ARCHNAME)}, \
        platform     => q{$(OSNAME)}, \
        tags         => q{$(OSNAME), $(ARCHNAME), $(LUA)}, \
        comments     => q{$(LUA)}, \
        username     => q{parrot-autobot}, \
        password     => q{qa_rocks}, \
        project_id   => $$project_id, \
        report_file  => [q{test_lua52.tar.gz}], \
        ] \
); \
if ($$response->code == 302) { \
    my ($$report_id) = $$response->content =~ /Reported .(\d+) added/i; \
    my $$report_url = $$server . q{/app/public_projects/report_details/} . $$report_id; \
    my $$project_url = $$server . q{/app/public_projects/smoke_reports/} . $$project_id; \
    print qq{Test report successfully sent to Smolder at\n$$report_url} \
      . qq{\nYou can see other recent reports at\n$$project_url .\n\n}; \
} \
else { \
    die qq{Could not upload report to Smolder at $$server} \
      . qq{\nHTTP CODE: } . $$response->code . q{ (} \
      . $$response->message . qq{)\n}; \
}

smolder: env
	-@prove --archive test_lua52.tar.gz --exec=$(LUA) *.t
	perl -e "$(upload_pl)"

