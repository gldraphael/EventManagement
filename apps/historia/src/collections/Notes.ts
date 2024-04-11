import { CollectionConfig } from 'payload/types';
import { admins, anyone } from '../access';
import { contentPeople } from '../fields/contentPeople';

export const Notes: CollectionConfig = {
  slug: 'notes',
  access: {
    read: anyone,
    create: admins,
    update: admins,
    delete: admins,
  },
  fields: [
    {
      name: 'title',
      type: 'text',
      required: true,
      admin: {
        description: 'The title of the note.',
      },
    },
    {
      name: 'image',
      type: 'upload',
      relationTo: 'media',
      admin: {
        description: 'An optional image related to the note.',
      },
    },
    {
      name: 'content',
      type: 'richText',
      admin: {
        description: 'The main content of the note.',
      },
    },
    contentPeople
  ],
};

