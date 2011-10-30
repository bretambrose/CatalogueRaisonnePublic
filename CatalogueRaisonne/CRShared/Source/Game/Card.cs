/*

	Card.cs

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
	public enum ECardColor
	{
		Invalid,

		Red,
		Green,
		Blue,
		White,
		Yellow
	}

	public enum ECardValue
	{
		Invalid,

		Multiplier1,
		Multiplier2,
		Multiplier3,

		Two,
		Two_v2,
		Three,
		Three_v2,
		Four,
		Four_v2,
		Five,
		Six,
		Seven,
		Eight,
		Nine,
		Ten
	}

	public enum ECardDelta
	{
		Invalid, 

		Add,
		Remove
	}

	public static class CCardUtils
	{
		public static int Compute_Card_Sort_Key( CCard card )
		{
			return ( ( int ) card.Color.ToString()[ 0 ] ) * 11 + (int) ( card.Value );
		}

		public static string Get_Card_Short_Form( CCard card )
		{
			return Get_Card_Short_Form( card.Color, card.Value );
		}

		public static string Get_Card_Short_Form( ECardColor color, ECardValue value )
		{
			return String.Format( "{0}{1}", color.ToString()[ 0 ], Get_Card_Score_Value_Short_Form( value ) );
		}

		public static int Get_Card_Score_Value( ECardValue card_value )
		{
			switch ( card_value )
			{
				case ECardValue.Two:
				case ECardValue.Two_v2:
					return 2;

				case ECardValue.Three:
				case ECardValue.Three_v2:
					return 3;

				case ECardValue.Four:
				case ECardValue.Four_v2:
					return 4;

				case ECardValue.Five:
					return 5;

				case ECardValue.Six:
					return 6;

				case ECardValue.Seven:
					return 7;

				case ECardValue.Eight:
					return 8;

				case ECardValue.Nine:
					return 9;

				case ECardValue.Ten:
					return 10;

				default:
					return 0;
			}
		}

		public static string Get_Card_Score_Value_Short_Form( ECardValue card_value )
		{
			switch ( card_value )
			{
				case ECardValue.Multiplier1:
					return "m1";
					
				case ECardValue.Multiplier2:
					return "m2";

				case ECardValue.Multiplier3:
					return "m3";

				case ECardValue.Two:
					return "2";

				case ECardValue.Two_v2:
					return "2v2";

				case ECardValue.Three:
					return "3";

				case ECardValue.Three_v2:
					return "3v2";

				case ECardValue.Four:
					return "4";

				case ECardValue.Four_v2:
					return "4v2";

				case ECardValue.Five:
					return "5";

				case ECardValue.Six:
					return "6";

				case ECardValue.Seven:
					return "7";

				case ECardValue.Eight:
					return "8";

				case ECardValue.Nine:
					return "9";

				case ECardValue.Ten:
					return "10";

				default:
					return "??";
			}
		}

		public static void Convert_Card_From_Short_Form( string short_form, out ECardColor color, out ECardValue value )
		{
			if ( short_form.Length < 2 )
			{
				color = ECardColor.Invalid;
				value = ECardValue.Invalid;
				return;
			}

			Extract_Color_From_Short_Form( short_form.Substring( 0, 1 ), out color );
			Extract_Value_From_Short_Form( short_form.Substring( 1 ), out value );
		}

		public static void Extract_Color_From_Short_Form( string color_short_form, out ECardColor color )
		{
			color = ECardColor.Invalid;

			if ( color_short_form.Length == 0 )
			{
				return;
			}

			switch ( color_short_form[ 0 ] )
			{
				case 'B':
				case 'b':
					color = ECardColor.Blue;
					break;

				case 'G':
				case 'g':
					color = ECardColor.Green;
					break;

				case 'R':
				case 'r':
					color = ECardColor.Red;
					break;

				case 'W':
				case 'w':
					color = ECardColor.White;
					break;

				case 'Y':
				case 'y':
					color = ECardColor.Yellow;
					break;

			}
		}

		public static void Extract_Value_From_Short_Form( string value_short_form, out ECardValue value )
		{
			value = ECardValue.Invalid;

			if ( value_short_form.Length == 0 )
			{
				return;
			}

			if ( value_short_form[ 0 ] == 'M' || value_short_form[ 0 ] == 'm' )
			{
				if ( value_short_form.Length != 2 )
				{
					return;
				}

				int index = 0;
				if ( !Int32.TryParse( value_short_form.Substring( 1 ), out index ) )
				{
					return;
				}

				switch ( index )
				{
					case 1:
						value = ECardValue.Multiplier1;
						break;

					case 2:
						value = ECardValue.Multiplier2;
						break;

					case 3:
						value = ECardValue.Multiplier3;
						break;

					default:
						break;
				}

				return;
			}

			if ( value_short_form.Length == 3 )
			{
				string suffix = value_short_form.Substring( 1 );
				if ( suffix == "v2" || suffix == "V2" )
				{
					int prefix_value = 0;
					if ( !Int32.TryParse( value_short_form.Substring( 0, 1 ), out prefix_value ) )
					{
						return;
					}

					switch ( prefix_value )
					{
						case 2:
							value = ECardValue.Two_v2;
							break;

						case 3:
							value = ECardValue.Three_v2;
							break;

						case 4:
							value = ECardValue.Four_v2;
							break;
					}
				}
			}
			else
			{
				int numerical_value = 0;
				if ( !Int32.TryParse( value_short_form, out numerical_value ) )
				{
					return;
				}

				switch ( numerical_value )
				{
					case 2:
						value = ECardValue.Two;
						break;

					case 3:
						value = ECardValue.Three;
						break;

					case 4:
						value = ECardValue.Four;
						break;

					case 5:
						value = ECardValue.Five;
						break;

					case 6:
						value = ECardValue.Six;
						break;

					case 7:
						value = ECardValue.Seven;
						break;

					case 8:
						value = ECardValue.Eight;
						break;

					case 9:
						value = ECardValue.Nine;
						break;

					case 10:
						value = ECardValue.Ten;
						break;

				}
			}
		}
				
		public static bool Is_Multiplier( ECardValue card_value )
		{
			switch ( card_value )
			{
				case ECardValue.Multiplier1:
				case ECardValue.Multiplier2:
				case ECardValue.Multiplier3:
					return true;

				default:
					return false;
			}
		}
	}

	public class CCard : ISimpleClonable< CCard >
	{
		// Construction
		public CCard( ECardColor color, ECardValue value )
		{
			Color = color;
			Value = value;
		}

		// Public Interface
		public CCard Clone()
		{
			return new CCard( Color, Value );
		}

		public override string ToString()
		{
			return String.Format( "( [CCard] Color = {0}, Value = {1} )", Color.ToString(), Value.ToString() );
		}

		// Properties
		public ECardColor Color { get; private set; }
		public ECardValue Value { get; private set; }
	}

}