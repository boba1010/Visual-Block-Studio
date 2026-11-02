using CSharpBlocksCompiler;
using CSharpBlocksCompiler.Emission;
using CSharpBlocksCompiler.Syntax;

var declaration = new VariableDeclarationSyntax
{
    Type = new TypeSyntax("int"),
    Identifier = new IdentifierSyntax("x"),
    Initializer = new LiteralExpressionSyntax(10, LiteralKind.Integer)
};

var assignment = new ExpressionStatementSyntax(
    new AssignmentExpressionSyntax(
        new IdentifierExpressionSyntax("x"),
        new LiteralExpressionSyntax(20, LiteralKind.Integer)));

var tree = SyntaxTree.Create(new CompilationUnitSyntax([declaration, assignment]));

var compilation = Compilation.Create(tree);

foreach (var diagnostic in compilation.GetDiagnostics())
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");

var model = compilation.GetSemanticModel(tree);

var emitter = new CSharpEmitter();
Console.WriteLine(emitter.Emit(model.SyntaxTree));