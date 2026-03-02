'use strict'

import * as vscode from 'vscode'

import {MoonSharpDebugConfigurationProvider} from './moonSharpDebugConfiguration'

export function activate(context: vscode.ExtensionContext) {
	context.subscriptions.push(
		vscode.debug.registerDebugConfigurationProvider(
			'moonsharp-lua',
			new MoonSharpDebugConfigurationProvider(() => {})
		)
	)
}

export function deactivate() {
}
