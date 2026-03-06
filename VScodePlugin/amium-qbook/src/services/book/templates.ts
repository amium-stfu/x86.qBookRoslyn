export function generateProgramClassContent(pageNames: string[]): string {
  const properties = pageNames
    .map((page) => `\t\tpublic static Definition${page}.qPage ${page} { get; } = new Definition${page}.qPage();`)
    .join('\n');

  const methodBody = (method: 'Initialize' | 'Run' | 'Destroy'): string => {
    if (!pageNames.length) {
      return '\t\t\t// Keine Pages registriert';
    }
    return pageNames.map((page) => `\t\t\t${page}.${method}();`).join('\n');
  };

  const propertyBlock = properties ? `${properties}\n\n` : '';

  return `namespace QB
{
\tpublic static class Program
\t{
${propertyBlock}\t\tpublic static void Initialize()
\t\t{
${methodBody('Initialize')}
\t\t}

\t\tpublic static void Run()
\t\t{
${methodBody('Run')}
\t\t}

\t\tpublic static void Destroy()
\t\t{
${methodBody('Destroy')}
\t\t}
\t}
}
`;
}

export function generateQPageTemplate(pageName: string): string {
  return `namespace Definition${pageName} { //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class qPage
    {
        //common fields/properties/methods/classes/types go here

        public void Initialize()
        {
            //initialization code goes here

        }

        public void Run()
        {
            //run/work code goes here

        }

        public void Destroy()
        {
            //destroy/cleanup code goes here
        }
    }
    //<CodeEnd>
}
`;
}

export function generateOPageTemplate(pageName: string): string {
  const document = {
    Name: pageName,
    Text: pageName,
    OrderIndex: 0,
    Hidden: false,
    Format: 'A4',
    Includes: [] as string[],
    CodeOrder: [] as string[],
    Section: '',
    Url: null,
  };
  return JSON.stringify(document, null, 2) + '\n';
}

export function generateSubcodeTemplate(pageName: string, codeName: string): string {
  return `namespace Definition${pageName}
{
    //<CodeStart>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using QB;

    public class ${codeName}
    {

    }
    //<CodeEnd>
}
`;
}
