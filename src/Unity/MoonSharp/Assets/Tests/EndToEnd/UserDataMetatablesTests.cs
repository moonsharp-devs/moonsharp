using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
    
    [TestFixture]
    public class UserDataMetatablesTests
    {

        class GenericUserDataTestClass
        {

            int a;
            public int b = 7;
            public int c = 11;

            public int x = 100;
            public int y = 150;
            public int z = 200;

            public void SetA(int a)
            {
                this.a = a;
            }

            public int GetA()
            {
                return a;
            }

            public string GetSome()
            {
                return "Some";
            }

            public string GetAnything()
            {
                return "Anything";
            }

        }

        Script CreateTestEnvironment() {
            Script s = new Script(CoreModules.Preset_Complete);
            var obj = new GenericUserDataTestClass();
            UserData.RegisterType<GenericUserDataTestClass>();
            s.Globals.Set("o", UserData.Create(obj));
            return s;
        }

        [Test]
        public void UserDataMetatable_NewIndexOverride() {
            var s = CreateTestEnvironment();

            var code = @"
                local backingTable = {}

                debug.setmetatable(o, { __newindex = function(ud, k, v)
                     backingTable[k] = table.concat({'ud = ', tostring(ud), ', k = ', k, ', v = ', v})
                end })

                local test = table.concat({'ud = ', tostring(o), ', k = a, v = 2'})

                o.a = 2

                return test == backingTable.a
                ";
            var result = s.DoString(code);
            
            Assert.AreEqual(DataType.Boolean, result.Type);
            Assert.AreEqual(DynValue.True, result);
        }

        [Test]
        public void UserDataMetatable_IndexOverride() {
            var s = CreateTestEnvironment();

            var code = @"
                local backingTable = { c = 12, d = 42 }

                debug.setmetatable(o, { __index = function(ud, k) 
                    return tostring(backingTable[k]) .. '!'
                end })

                return table.concat({'check ', tostring(o.b), ' ', tostring(o.c), ' ', tostring(o.d)})
            ";

            var result = s.DoString(code);
            
            Assert.AreEqual(DataType.String, result.Type);
            Assert.AreEqual("check 7 11 42!", result.String);
        }

        [Test]
        public void UserDataMetatable_Table_Recursive() {
            var s = CreateTestEnvironment();
            
            var code = @"
                local t = { c = 12, d = 42, n = -100 }
                local mt = { __index = t, m = 99, n = 199, o = 200 } 

                debug.setmetatable(o, mt)

                return table.concat({'check ',
                     tostring(o.b), ' ', tostring(o.c), ' ', tostring(o.d), ' ',
                     tostring(o.m), ' ', tostring(o.n), ' ', tostring(o.o)})
            ";

            var result = s.DoString(code);
            
            Assert.AreEqual(DataType.String, result.Type);
            Assert.AreEqual("check 7 11 42 nil -100 nil", result.String);
        }

        [Test]
        public void UserDataMetatable_UserDataMethodsOverride() {
            var s = CreateTestEnvironment();
            
            var code = @"
                local t = {
                    f = 999,
                    GetSome = function() return 'squirrels' end,
                    HaveFun = function() return 'so much fun wow' end
                }
                local mt = { __index = t } 

                debug.setmetatable(o, mt)

                o:SetA(3)

                return table.concat({'check ',
                     tostring(o.GetA()), ' ', tostring(o.GetSome()), ' ', tostring(o.GetAnything()), ' ', tostring(o.f), ' ', tostring(o.HaveFun())
                })
            ";

            var result = s.DoString(code);

            Assert.AreEqual(DataType.String, result.Type);
            Assert.AreEqual("check 3 Some Anything 999 so much fun wow", result.String);
        }
    }
}