//
//  main.swift
//  AltNetworkUtility.macOS.SwiftPrivilegedHelper
//
//  Created by Sören Kuklau on 29.01.22.
//

import Foundation

//NSApplication.init(coder: <#T##NSCoder#>)
//print("Hello, World!")

let listenerDelegate = ListenerDelegate()

let listener = NSXPCListener.service()
listener.delegate = listenerDelegate
listener.resume()

RunLoop.main.run()

class ListenerDelegate : NSObject, NSXPCListenerDelegate {
    func listener(_ listener: NSXPCListener, shouldAcceptNewConnection newConnection: NSXPCConnection) -> Bool {
        
        print("new connection")
        
        return true
    }
}
