using System.IO;
/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using XLua;

namespace XLuaTest
{
    public class Helloworld : MonoBehaviour
    {
        // Use this for initialization
        LuaEnv _env;
        void Start()
        {
            print(Application.dataPath);
            _env = new LuaEnv();
            _env.DoString("CS.UnityEngine.Debug.Log('hello world')");
            _env.DoString(@"
                print('aaa')
                print('hello ...')
                package.cpath = package.cpath .. ';/Users/luowentao/.vscode/extensions/tangzx.emmylua-0.5.8/debugger/emmy/mac/arm64/emmy_core.dylib'
                print(package.cpath)
                local dbg = require('emmy_core')
                print('dbg = ', dbg)
                dbg.tcpConnect('localhost', 9966)
            ");
            _env.AddLoader(CustomMyLoader);
            _env.DoString("require 'aa'");

        }

        private byte[] CustomMyLoader(ref string fileName)
        {
            string luaPath = Application.dataPath + "/XLua/Examples/01_Helloworld/" + fileName + ".lua";
            string strLuaContent = File.ReadAllText(luaPath);
            byte[] result = System.Text.Encoding.UTF8.GetBytes(strLuaContent);
            return result;
        }


        // Update is called once per frame
        void Update()
        {

        }
        
        private void OnDestroy() {
            _env.Dispose();
        }
    }
}
