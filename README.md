<h2>Reasoning Engine: Transparent and Auditable AI Reasoning with LLM Support</h1>

<h2>Overview</h2>
<p>
    An AI system designed to exceed human-level problem-solving in general contexts by: 

1. encoding knowledge in a human-readable graph,
2. performing explicit and auditable reasoning,
3. incorporating new information via data ingestion algorithms assisted by LLMs, and
4. enhancing knowledge accuracy and predictive power through refinement algorithms.
</p>

<h2>Features</h2>
<ul>

</ul>

<h2>Acknowledgments</h2>
<ul>
    <li><a href="https://github.com/conormckenzie/reasoning-engine">Original reasoning-engine repository</a></li>
</ul>

<p>For more information, feel free to reach out or open an issue in the repository.</p>

</body>
</html>

## Configuration
- **Environment Variables**: The application relies on the following environment variable, which should be set in the `.env` file:
   - `DATA_FOLDER_PATH`: The path to the main data directory where graph data and other files will be stored. Example:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
   - Ensure this environment variable is set before running the application.

## Getting Started
1. Ensure that the necessary environment variables are set in the `.env` file.
   - Example `.env` file:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
2. Build the project using the .NET CLI:
   \`\`\`bash
   dotnet build
   \`\`\`

3. Run the project:
   \`\`\`bash
   dotnet run --project reasoningEngine
   \`\`\`
