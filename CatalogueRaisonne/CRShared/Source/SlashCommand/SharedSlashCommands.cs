﻿/*

	SharedSlashCommands.cs

	(c) Copyright 2010-2011, Bret Ambrose (mailto:bretambrose@gmail.com).

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
*/

using System;

namespace CRShared
{
	[SlashCommand( "Help", AllowSymbols = true )]
	public class CHelpSlashCommand : CSlashCommand
	{
		public CHelpSlashCommand()
		{
			CommandGroupOrName = "";
		}
						
		[SlashParam( ESlashCommandParameterType.Optional )]
		public string CommandGroupOrName { get; private set; }
	}

	[SlashCommand( "SetLogLevel" )]
	public class CSetLogLevelSlashCommand : CSlashCommand
	{
		public CSetLogLevelSlashCommand()
		{
			Level = ELogLevel.Medium;
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.Logging; } }
						
		[SlashParam( ESlashCommandParameterType.Required )]
		public ELogLevel Level { get; private set; }
	}

	[SlashCommand( "Crash" )]
	public class CCrashSlashCommand : CSlashCommand
	{
		public CCrashSlashCommand()
		{
		}

		public override ESlashCommandGroup CommandGroup { get { return ESlashCommandGroup.ProcessControl; } }
						
	}
}