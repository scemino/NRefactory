//
// PossibleParenthesizedExpression.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp;

namespace ICSharpCode.NRefactory.PatternMatching
{
	/// <summary>
	/// Matches an expression regardless of it has redundant parentheses or not
	/// </summary>
	public class PossibleParenthesizedExpression : Expression
	{
		readonly Expression expr;

		public PossibleParenthesizedExpression(Expression expr)
		{
			this.expr = expr;
		}

		public override void AcceptVisitor (IAstVisitor visitor)
		{
			throw new InvalidOperationException();
		}

		public override T AcceptVisitor<T> (IAstVisitor<T> visitor)
		{
			throw new InvalidOperationException();
		}

		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			throw new InvalidOperationException();
		}

		protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
		{
			do {
				var parenthesized = other as ParenthesizedExpression;
				if (parenthesized == null)
					return expr.DoMatch(other, match);
				other = parenthesized.Expression;
			} while (other != null);
			return false;
		}

	}
}
