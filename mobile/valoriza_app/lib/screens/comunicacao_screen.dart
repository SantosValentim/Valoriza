import 'package:flutter/material.dart';

/// Checklist local de exemplo (API de comunicação pode ser ligada depois)
class ComunicacaoScreen extends StatelessWidget {
  const ComunicacaoScreen({super.key});

  @override
  Widget build(BuildContext context) {
    const itens = [
      'Revisei o vocabulário do comunicado (sem estereótipos)',
      'Garanti exemplos e imagens inclusivos',
      'No feedback, foquei em comportamentos e resultados',
      'Compartilhei orientação antirracista com a equipe esta semana',
    ];
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Text('Comunicação inclusiva', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 8),
        const Text('Checklist para gestores', style: TextStyle(color: Colors.grey)),
        const SizedBox(height: 16),
        ...itens.map((t) => Card(child: ListTile(leading: const Icon(Icons.check_box_outline_blank), title: Text(t)))),
      ],
    );
  }
}