//
// ReplaceWithOfTypeIssue.cs
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
using System.Linq;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Refactoring;

namespace ICSharpCode.NRefactory.CSharp.Refactoring
{
	[IssueDescription("Replace with OfType<T>",
	                  Description = "Replace with call to OfType<T>",
	                  Category = IssueCategories.PracticesAndImprovements,
	                  Severity = Severity.Suggestion,
	                  ResharperDisableKeyword = "ReplaceWithOfType")]
	public class ReplaceWithOfTypeIssue : GatherVisitorCodeIssueProvider
	{
		static readonly AstNode selectNotNullPattern =
			new InvocationExpression(
				new MemberReferenceExpression(new AnyNode("target"), "SelectNotNull"),
				new LambdaExpression {
					Parameters = { new NamedParameterDeclaration ("param1", new AnyType (true, "paramType"), Pattern.AnyString) },
					Body = new PossibleParenthesizedExpression (new AsExpression(new AnyNode("expr1"), new AnyNode("type")))
				}
			);

		static readonly AstNode wherePatternCase1 =
			new InvocationExpression(
				new MemberReferenceExpression(
					new InvocationExpression(
						new MemberReferenceExpression(new AnyNode("target"), "Where"),
						new LambdaExpression {
							Parameters = { new NamedParameterDeclaration ("param1", new AnyType (true, "paramType"), Pattern.AnyString) },
							Body = new PossibleParenthesizedExpression (new IsExpression(new AnyNode("expr1"), new AnyNode("type")))
						}
					), "Select"),
				new LambdaExpression {
					Parameters = { new NamedParameterDeclaration ("param2", new AnyType (true, "paramType"), Pattern.AnyString) },
					Body = new PossibleParenthesizedExpression (new AsExpression(new PossibleParenthesizedExpression (new AnyNode("expr2")), new Backreference("type")))
				}
		);

		static readonly AstNode wherePatternCase2 =
			new InvocationExpression(
				new MemberReferenceExpression(
					new InvocationExpression(
						new MemberReferenceExpression(new AnyNode("target"), "Where"),
						new LambdaExpression {
							Parameters = { new NamedParameterDeclaration ("param1", new AnyType (true, "paramType"), Pattern.AnyString) },
							Body = new PossibleParenthesizedExpression (new IsExpression(new PossibleParenthesizedExpression (new AnyNode("expr1")), new AnyNode("type")))
						}
					), "Select"),
				new LambdaExpression {
					Parameters = { new NamedParameterDeclaration ("param2", new AnyType (true, "paramType"), Pattern.AnyString) },
					Body = new PossibleParenthesizedExpression (new CastExpression(new Backreference("type"), new PossibleParenthesizedExpression (new AnyNode("expr2"))))
				}
		);

		protected override IGatherVisitor CreateVisitor(BaseRefactoringContext context)
		{
			return new GatherVisitor(context);
		}

		class GatherVisitor : GatherVisitorBase<ReplaceWithOfTypeIssue>
		{
			public GatherVisitor (BaseRefactoringContext ctx) : base (ctx)
			{
			}

			bool CheckParameterMatches(IEnumerable<INode> param, IEnumerable<INode> expr)
			{
				var p = param.Single() as ParameterDeclaration;
				var e = expr.Single();

				if (p == null)
					return false;
				if (e is IdentifierExpression)
					return p.Name == ((IdentifierExpression)e).Identifier;
				return false;
			}

			public override void VisitInvocationExpression (InvocationExpression anyInvoke)
			{
				base.VisitInvocationExpression (anyInvoke);
				var match = selectNotNullPattern.Match (anyInvoke);
				if (!match.Success) {
					match = wherePatternCase1.Match (anyInvoke);
					if (!match.Success) {
						match = wherePatternCase2.Match (anyInvoke);
						if (!match.Success)
							return;
					}
					if (!CheckParameterMatches(match.Get("param1"), match.Get("expr1")) || 
					    !CheckParameterMatches(match.Get("param2"), match.Get("expr2")))
						return;
				} else {
					if (!CheckParameterMatches(match.Get("param1"), match.Get("expr1")))
						return;
				}
				AddIssue (
					anyInvoke,
					ctx.TranslateString("Replace with OfType<T>"),
					ctx.TranslateString("Replace with call OfType<T>"),
					script => {
						var target = match.Get<Expression>("target").Single().Clone ();
						var type = match.Get<AstType>("type").Single().Clone();
						script.Replace(anyInvoke, new InvocationExpression(new MemberReferenceExpression(target, "OfType", type)));
					}
				);
			}

		}

	}
}
