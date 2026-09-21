import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../services/sync_service.dart';

class DenunciaScreen extends StatefulWidget {
  const DenunciaScreen({super.key});

  @override
  State<DenunciaScreen> createState() => _DenunciaScreenState();
}

class _DenunciaScreenState extends State<DenunciaScreen> {
  final _formKey = GlobalKey<FormState>();
  final _relatoController = TextEditingController();
  final _api = ApiService();

  String _tipo = 'Racismo';
  bool _anonima = true;
  bool _enviando = false;

  final List<String> _tipos = ['Racismo', 'Discriminação', 'Assédio', 'Outro'];

  Future<void> _enviar() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _enviando = true);

    final dadosDenuncia = {
      'tipo': _tipo,
      'relato': _relatoController.text.trim(),
      'anonima': _anonima,
    };

    try {
      // Tentativa online direta
      await _api.enviarDenuncia(
        tipo: dadosDenuncia['tipo'] as String,
        relato: dadosDenuncia['relato'] as String,
        anonima: dadosDenuncia['anonima'] as bool,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Denúncia registrada online com sucesso!'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context);
      }
    } on Exception catch (e) {
      // Intercepta falhas de rede do emulador (SocketException / Connection refused)
      if (e.toString().contains('SocketException') || e.toString().contains('Connection refused')) {
        
        // Grava no SharedPreferences
        await SyncService().salvarOffline(dadosDenuncia);

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Você está sem internet! O relato foi salvo localmente e será enviado automaticamente assim que a rede voltar.'),
              backgroundColor: Colors.orange,
              duration: Duration(seconds: 5),
            ),
          );
          Navigator.pop(context);
        }
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Erro na API: $e'), backgroundColor: Colors.red),
          );
        }
      }
    } finally {
      if (mounted) setState(() => _enviando = false);
    }
  }


  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Canal de Denúncias')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'Este canal é seguro e confidencial.\nVocê pode denunciar de forma anônima.',
                style: TextStyle(color: Colors.grey),
              ),
              const SizedBox(height: 24),
              DropdownButtonFormField<String>(
                value: _tipo,
                decoration: const InputDecoration(
                  labelText: 'Tipo de denúncia',
                  border: OutlineInputBorder(),
                ),
                items: _tipos
                    .map((t) => DropdownMenuItem(value: t, child: Text(t)))
                    .toList(),
                onChanged: (v) => setState(() => _tipo = v!),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _relatoController,
                maxLines: 6,
                decoration: const InputDecoration(
                  labelText: 'Relato',
                  hintText: 'Descreva o que aconteceu...',
                  border: OutlineInputBorder(),
                  alignLabelWithHint: true,
                ),
                validator: (v) {
                    if (v == null || v.trim().isEmpty) {
                      return 'Informe o relato';
                    }
                    if (v.trim().length < 20) {
                      return 'O relato deve ter pelo menos 20 caracteres';
                    }
                    if (v.trim().length > 3000) {
                      return 'O relato não pode passar de 3000 caracteres';
                    }
                    return null; // Texto válido
                  },
                ),
              const SizedBox(height: 16),
              SwitchListTile(
                title: const Text('Denúncia anônima'),
                subtitle: const Text('Seu nome não será revelado'),
                value: _anonima,
                onChanged: (v) => setState(() => _anonima = v),
              ),
              const SizedBox(height: 24),
              SizedBox(
                height: 50,
                child: ElevatedButton(
                  onPressed: _enviando ? null : _enviar,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.red.shade700,
                    foregroundColor: Colors.white,
                  ),
                  child: _enviando
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text('Enviar denúncia'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
