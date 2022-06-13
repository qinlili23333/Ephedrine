window.Ephedrine = {
    msgStatus: code => {
        console.log(code);
        Ephedrine.processCode(code);
    },
    msgResult: result => {
        console.log(result);
        Ephedrine.processResult(result);
    },
    request: (action, arg1, arg2, arg3, arg4, arg5) => {
        window.chrome.webview.postMessage({ Action: action, Arg1: arg1, Arg2: arg2, Arg3: arg3, Arg4: arg4, Arg5: arg5 });
    },
    processCode: code => {},
    processResult: result => {},
    msgAsync: (action, arg1, arg2, arg3, arg4, arg5, forResult) => {
        return new Promise(resolve => {
            Ephedrine.request(action, arg1, arg2, arg3, arg4, arg5);
            if (forResult) {
                let result1 = "Empty";
                let code1 = 0;
                const triggerCheck = () => {
                    if (!(result1 === "Empty") && !(code1 === 0)) {
                        resolve({
                            result: result1,
                            code: code1
                        })
                    }
                }
                Ephedrine.processResult = result => {
                    result1 = result;
                    triggerCheck();
                }
                Ephedrine.processCode = code => {
                    code1 = code;
                    triggerCheck();
                }
            } else {
                Ephedrine.processCode = code => {
                    resolve(code);
                }
            }
        })
    },
    Ready: !!window.chrome.webview,
    Actions: {
        StartService: async(elevated) => {
            switch (await Ephedrine.msgAsync("StartService", elevated ? "Admin" : "User")) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            running: false,
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 90:
                    {
                        //Start Fail 
                        return {
                            code: 90,
                            success: false,
                            running: false,
                            msg: "Fail"
                        }
                        break;
                    };
                case 91:
                    {
                        //Start Success 
                        return {
                            code: 91,
                            success: true,
                            running: true,
                            msg: "Success"
                        }
                        break;
                    };
                case 93:
                    {
                        //Start Fail 
                        return {
                            code: 93,
                            success: false,
                            running: true,
                            msg: "Already Running"
                        }
                        break;
                    }
            }
        },
        List: async(path) => {
            let result = await Ephedrine.msgAsync("List", path, null, null, null, null, true)
            switch (result.code) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            result: "",
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 30:
                    {
                        //List Failed
                        return {
                            code: 40,
                            success: false,
                            result: "",
                            msg: "List Failed"
                        }
                        break;
                    };
                case 31:
                    {
                        //List Success
                        return {
                            code: 41,
                            success: true,
                            result: JSON.parse(atob(result.result)),
                            msg: "Success"
                        }
                        break;
                    };
            }
        },
        Install: {
            zip: async(link, location) => {
                switch (await Ephedrine.msgAsync("Install", "zip", link, location)) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 10:
                        {
                            //Zip Patch Fail
                            return {
                                code: 10,
                                success: false,
                                msg: "Failed To Patch"
                            }
                            break;
                        };
                    case 11:
                        {
                            //Success Zip Patch
                            return {
                                code: 11,
                                success: true,
                                msg: "Success"
                            }
                            break;
                        };
                    case 13:
                        {
                            //Zip Patch Permission Denied
                            return {
                                code: 13,
                                success: false,
                                msg: "Permission Denied"
                            }
                            break;
                        }
                }
            },
            run: async(link, location, admin, argu) => {
                switch (await Ephedrine.msgAsync("Install", "run", link, location, admin ? "Admin" : "User", argu)) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 16:
                        {
                            //Download And Run Success 
                            return {
                                code: 16,
                                success: true,
                                msg: "Success"
                            }
                            break;
                        };
                    case 17:
                        {
                            //No Permission Start
                            return {
                                code: 17,
                                success: false,
                                msg: "No Permission"
                            }
                            break;
                        };
                    case 18:
                        {
                            //Run Fail
                            return {
                                code: 18,
                                success: false,
                                msg: "Run Failed"
                            }
                            break;
                        }
                }
            },
            save: async(link, name) => {
                switch (await Ephedrine.msgAsync("Install", "save", link, name)) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 15:
                        {
                            //Download Success Without Verification
                            return {
                                code: 15,
                                success: true,
                                msg: "Success With No Verification"
                            }
                            break;
                        };
                }
            }
        },
        Select: async(folder) => {
            let result = await Ephedrine.msgAsync("Select", folder ? "Folder" : "File", null, null, null, null, true)
            switch (result.code) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            result: "",
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 40:
                    {
                        //Select Canceled
                        return {
                            code: 40,
                            success: false,
                            result: "",
                            msg: "Select Canceled"
                        }
                        break;
                    };
                case 41:
                    {
                        //Select Success
                        return {
                            code: 41,
                            success: true,
                            result: result.result,
                            msg: "Success"
                        }
                        break;
                    };
            }
        },
        KillProcess: async(name) => {
            switch (await Ephedrine.msgAsync("KillProcess", name.replace(".exe", ""))) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 70:
                    {
                        //Nothing To Kill 
                        return {
                            code: 70,
                            success: false,
                            running: false,
                            msg: "Nothing Found"
                        }
                        break;
                    };
                case 71:
                    {
                        //Kill Success 
                        return {
                            code: 71,
                            success: true,
                            running: false,
                            msg: "Success"
                        }
                        break;
                    }

            }
        },
        Verify: async(method, file) => {
            let result = await Ephedrine.msgAsync("Verify", method, file, null, null, null, true)
            switch (result.code) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            result: "",
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 80:
                    {
                        //No File
                        return {
                            code: 80,
                            success: false,
                            result: "",
                            msg: "No Such File"
                        }
                        break;
                    }
                case 81:
                    {
                        //Verify Finish
                        return {
                            code: 81,
                            success: true,
                            result: result.result,
                            msg: "Success"
                        }
                        break;
                    };
                case 82:
                    {
                        //Unsupport Method
                        return {
                            code: 82,
                            success: false,
                            result: "",
                            msg: "Unsupport Method"
                        }
                        break;
                    };
            }
        },
        Run: async(program, argu, admin, intent) => {
            switch (await Ephedrine.msgAsync("Run", program, argu, intent ? "Intent" : (admin ? "Admin" : "User"))) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            result: "",
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 60:
                    {
                        //Run Fail
                        return {
                            code: 60,
                            success: false,
                            msg: "Run Failed"
                        }
                        break;
                    }
                case 61:
                    {
                        //VRun Success
                        return {
                            code: 61,
                            success: true,
                            msg: "Success"
                        }
                        break;
                    };
            }
        },
        Delete: async(file) => {
            switch (await Ephedrine.msgAsync("Delete", file)) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            result: "",
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 50:
                    {
                        //File Not Found
                        return {
                            code: 50,
                            success: false,
                            found: false,
                            msg: "File Not Found"
                        }
                        break;
                    }
                case 51:
                    {
                        //Delete Success
                        return {
                            code: 51,
                            success: true,
                            found: true,
                            msg: "Success"
                        }
                        break;
                    };
                case 52:
                    {
                        //Delete Fail
                        return {
                            code: 52,
                            success: false,
                            found: true,
                            msg: "Fail"
                        }
                        break;
                    };
            }
        }

    }
}