/* Detalhe da trilha: lista conteúdos, abre texto/vídeo/quiz */
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../services/api_service.dart';

class TrilhaDetalheScreen extends StatefulWidget {
  final int trilhaId;
  const TrilhaDetalheScreen({super.key, required this.trilhaId});

  @override
  State<TrilhaDetalheScreen> createState() => _TrilhaDetalheScreenState();
}

class _TrilhaDetalheScreenState extends State<TrilhaDetalheScreen> {
  final _api = ApiService();
  Map<String, dynamic>? _trilha;
  bool _loading = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _erro = null;
    });
    try {
      _trilha = await _api.getTrilha(widget.trilhaId);
    } catch (e) {
      _erro = e.toString();
    }
    setState(() => _loading = false);
  }

  List<dynamic> get _conteudos {
    final c = _trilha?['conteudos'];
    if (c is List) return c;
    return [];
  }

  void _abrirConteudo(Map<String, dynamic> c) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => ConteudoScreen(
          conteudo: c,
          trilhaId: widget.trilhaId,
          onConcluido: _load,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }
    if (_erro != null || _trilha == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Trilha')),
        body: Center(child: Text(_erro ?? 'Trilha não encontrada')),
      );
    }

    final titulo = _trilha!['titulo']?.toString() ?? 'Trilha';
    final desc = _trilha!['descricao']?.toString() ?? '';
    final carga = _trilha!['cargaHoraria'] ?? 0;
    final nivel = _trilha!['nivel']?.toString() ?? '';

    return Scaffold(
      appBar: AppBar(title: Text(titulo)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (desc.isNotEmpty)
            Text(desc, style: TextStyle(color: Colors.grey.shade700)),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            children: [
              Chip(label: Text('$carga min')),
              if (nivel.isNotEmpty) Chip(label: Text(nivel)),
              Chip(label: Text('${_conteudos.length} conteúdos')),
            ],
          ),
          const SizedBox(height: 16),
          Text('Conteúdos', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          if (_conteudos.isEmpty)
            const Card(
              child: ListTile(title: Text('Nenhum conteúdo nesta trilha.')),
            )
          else
            ..._conteudos.map((raw) {
              final c = Map<String, dynamic>.from(raw as Map);
              final tipo = c['tipo']?.toString() ?? 'Texto';
              final dur = c['duracaoMinutos'] ?? 0;
              final ordem = c['ordem'] ?? '';
              IconData icon = Icons.article_outlined;
              if (tipo == 'Video') icon = Icons.play_circle_outline;
              if (tipo == 'Quiz') icon = Icons.quiz_outlined;

              return Card(
                margin: const EdgeInsets.only(bottom: 8),
                child: ListTile(
                  leading: CircleAvatar(
                    backgroundColor: const Color(0xFF1a1a2e).withValues(alpha: 0.1),
                    child: Icon(icon, color: const Color(0xFF1a1a2e)),
                  ),
                  title: Text('$ordem. ${c['titulo'] ?? ''}'),
                  subtitle: Text('$tipo · $dur min'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => _abrirConteudo(c),
                ),
              );
            }),
        ],
      ),
    );
  }
}

/* Tela de um conteúdo (texto / vídeo / quiz) */
class ConteudoScreen extends StatefulWidget {
  final Map<String, dynamic> conteudo;
  final int trilhaId;
  final VoidCallback onConcluido;

  const ConteudoScreen({
    super.key,
    required this.conteudo,
    required this.trilhaId,
    required this.onConcluido,
  });

  @override
  State<ConteudoScreen> createState() => _ConteudoScreenState();
}

class _ConteudoScreenState extends State<ConteudoScreen> {
  final _api = ApiService();
  String? _respostaQuiz;
  bool _enviando = false;
  String? _feedback;

  String get _tipo => widget.conteudo['tipo']?.toString() ?? 'Texto';

  Future<void> _abrirVideo() async {
    final url = widget.conteudo['urlVideo']?.toString();
    if (url == null || url.isEmpty) return;
    final uri = Uri.tryParse(url);
    if (uri != null && await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  Future<void> _concluir({String? resposta}) async {
    setState(() {
      _enviando = true;
      _feedback = null;
    });
    try {
      final id = widget.conteudo['id'] as int;
      await _api.concluirConteudo(id, respostaQuiz: resposta);
      setState(() => _feedback = 'Conteúdo marcado como concluído!');
      widget.onConcluido();
    } catch (e) {
      setState(() => _feedback = 'Erro: $e');
    }
    setState(() => _enviando = false);
  }

  @override
  Widget build(BuildContext context) {
    final c = widget.conteudo;
    final titulo = c['titulo']?.toString() ?? 'Conteúdo';
    final texto = c['texto']?.toString() ?? '';

    return Scaffold(
      appBar: AppBar(title: Text(titulo)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Chip(label: Text(_tipo)),
          const SizedBox(height: 12),

          // Texto / pergunta
          if (texto.isNotEmpty)
            Text(texto, style: const TextStyle(fontSize: 16, height: 1.45)),

          // Vídeo
          if (_tipo == 'Video' && (c['urlVideo']?.toString().isNotEmpty ?? false)) ...[
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: _abrirVideo,
              icon: const Icon(Icons.play_arrow),
              label: const Text('Assistir vídeo'),
            ),
          ],

          // Quiz
          if (_tipo == 'Quiz') ...[
            const SizedBox(height: 20),
            const Text('Escolha a resposta:', style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            ...['A', 'B', 'C', 'D'].map((letra) {
              final opcao = c['opcao$letra']?.toString() ?? '';
              if (opcao.isEmpty) return const SizedBox.shrink();
              return RadioListTile<String>(
                value: letra,
                groupValue: _respostaQuiz,
                onChanged: (v) => setState(() => _respostaQuiz = v),
                title: Text('$letra) $opcao'),
              );
            }),
          ],

          const SizedBox(height: 24),
          if (_feedback != null)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(
                _feedback!,
                style: TextStyle(
                  color: _feedback!.startsWith('Erro') ? Colors.red : Colors.green.shade700,
                ),
              ),
            ),

          FilledButton(
            onPressed: _enviando
                ? null
                : () {
                    if (_tipo == 'Quiz') {
                      if (_respostaQuiz == null) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Selecione uma resposta')),
                        );
                        return;
                      }
                      _concluir(resposta: _respostaQuiz);
                    } else {
                      _concluir();
                    }
                  },
            child: _enviando
                ? const SizedBox(
                    height: 22,
                    width: 22,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : Text(_tipo == 'Quiz' ? 'Enviar resposta' : 'Marcar como concluído'),
          ),
        ],
      ),
    );
  }
}